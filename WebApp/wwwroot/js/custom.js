// Inicijalizacija
$(document).ready(function () {

    // Inicijaliziramo datume
    $('#dateDeparture,#dateArrival').datepicker({
        autoclose: true,
        language: 'hr',
        format: 'dd.mm.yyyy.',
        todayHighlight: true,
        todayBtn: true,
        weekStart: 1
    });
    $('#dateDeparture').datepicker('update', moment().format('DD.MM.YYYY'));
    $('#dateArrival').datepicker('update', moment().add(1, 'day').format('DD.MM.YYYY'));

    // Namještamo defaultnu vrijednost u broj putnika
    $('#personNum').val(1);

    // Refreshamo selectpickere i dodajemo im event listenere
    $('#airportDepartment,#airportArrival').selectpicker('refresh');

    $('#airportDepartmentDiv .form-control input').on('keyup', function () {
        reloadAirport("#airportDepartment", $(this).val());
    });

    $('#airportArrivalDiv .form-control input').on('keyup', function () {
        reloadAirport("#airportArrival", $(this).val());
    });

});
//-----------------------------------------


// Funkcija za učitati aerodrome prema traženoj riječi
function reloadAirport(selectId, key) {

    // Slanje ajax zahtjeva
    $.ajax({
        type: "GET",
        url: "/Flight/GetAirportByKey",
        data: {
            key: key
        },
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {

            // Pretvaramo podatke iz JSON-a
            var airports = jQuery.parseJSON(data);
            if (airports.length === 0) {
                return;
            }

            // Stvaramo string koji ćemo dodati u izbornik
            var appendStr = '';
            for (var i in airports) {
                appendStr += '<option value="' + airports[i].iata + '">' + airports[i].iata + " - " + airports[i].name + '</option>';
            }

            // Dodajemo u izbornik
            $(selectId)
                .html(appendStr)
                .selectpicker('refresh');

        },
        error: function () {
            // Izvještavamo korisnika za grešku
            alert("Dogodila se pogreška na poslužitelju !");
        }
    });
}
//-----------------------------------------


// Klik na gumb "Pretraži odmah" korisnika šalje na izbor
$('#btnSearchNow').click(function () {

    $('#airportDepartment').selectpicker('toggle');

});
//-----------------------------------------


// Na izmjeni broja provjeravamo valjanost
$('#personNum').change(function () {

    var persons = $('#personNum').val();

    // Ako je vrijednost otišla ispod 1 namještamo na 1
    if (persons < 1) {
        $('#personNum').val(1);
    }
    // Ako je otišla preko 50 namještamo na 50
    else if (persons > 50) {
        $('#personNum').val(50);
    }

    // Brišemo klasu koja ga crveni
    $('#personNum').parent().removeClass('has-error');
});
//-----------------------------------------


// Na izmjeni u izborima aerodroma
$('#airportDepartment,#airportArrival').change(function () {

    // Ako vrijednost nije null brišemo grešku
    if ($(this).val() !== null) {
        $(this).parent().parent().removeClass('has-error');
    }

});
//-----------------------------------------


// Na izmjeni u datumima
$('#dateDeparture,#dateArrival').change(function () {

    var dateDeparture = moment($('#dateDeparture').val(), 'DD.MM.YYYY.');
    var dateArrival = moment($('#dateArrival').val(), 'DD.MM.YYYY.');

    // Radimo provjeru ako možemo maknuti error
    if (moment(dateDeparture).startOf('day') >= moment().startOf('day') && dateDeparture < dateArrival) {
        $('#dateDeparture').parent().removeClass('has-error');
    }

    // Radimo provjeru ako možemo maknuti error
    if (dateArrival > dateDeparture) {
        $('#dateArrival').parent().removeClass('has-error');
    }
});
//-----------------------------------------


// Kada korisnik klikne na gumb za pretraživanje
$('#btnSearch').click(function () {

    // Učitavamo vrijednosti
    var departure = $('#airportDepartment').val();
    var arrival = $('#airportArrival').val();
    var dateDeparture = moment($('#dateDeparture').val(), 'DD.MM.YYYY.');
    var dateArrival = moment($('#dateArrival').val(), 'DD.MM.YYYY.');
    var personNum = $('#personNum').val();
    var currency = $('#travelCurrency').val();

    // Provjeravamo vrijednosti
    if (departure === null) {
        $('#airportDepartment').parent().parent().addClass('has-error');
    }
    if (arrival === null) {
        $('#airportArrival').parent().parent().addClass('has-error');
    }
    if (moment(dateDeparture).startOf('day') < moment().startOf('day') || dateDeparture >= dateArrival) {
        $('#dateDeparture').parent().addClass('has-error');
    }
    if (dateArrival <= dateDeparture) {
        $('#dateArrival').parent().addClass('has-error');
    }
    if (personNum < 1 || personNum > 50) {
        $('#personNum').parent().addClass('has-error');
    }

    // Ako postoji ijedna klasa .has-error zaustavljamo
    if ($('.has-error').length) {
        return;
    }

    // Skrivamo sve panel-e i prikazujemo sa loaderom
    $('.panel').fadeOut();
    $('#loader-panel').fadeIn();
    $('#table-panel tbody').empty();

    // Ako je sve u redu šaljemo zahtjev na server za listom
    $.ajax({
        type: "GET",
        url: "/Flight/Search",
        data: {
            origin: departure,
            destination: arrival,
            departure: moment(dateDeparture).format('YYYY/MM/DD HH:mm:ss'),
            return_date: moment(dateArrival).format('YYYY/MM/DD HH:mm:ss'),
            adults: personNum,
            currency: currency
        },
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {

            // Skrivamo panel od učitavanja
            $('#loader-panel').fadeOut();

            // Ako se dogodila neka pogreška
            if (data === "Error") {
                alert("Dogodila se pogreška na poslužitelju !");
                return;
            }

            // Deparsiramo podatke
            var offers = jQuery.parseJSON(data);

            // Provjeravamo dali imamo išta podataka u listi, ako nismo prikazujemo da nema podataka
            if (offers.length === 0 || data === "") {
                $('#noresult-panel').fadeIn();
                return;
            }

            // Prikazujemo tablicu u funkciji radi preglednosti
            displayOffersData(offers);

        },
        error: function () {
            // Skrivamo panel od učitavanja
            $('#loader-panel').fadeOut();

            // Izvještavamo korisnika za grešku
            alert("Dogodila se pogreška na poslužitelju !");
        }
    });

});
//-----------------------------------------


// Funkcija za prikaz podataka u tablici
function displayOffersData(data) {

    // Pripremamo za dodavanje
    $('#table-panel').fadeIn();
    var currency = data.currency;
    var count = 0;

    // Prolazimo kroz svaku ponudu
    for (var i in data.results) {

        // Uzimamo podatke u privremene varijable
        let offer = data.results[i].itineraries;
        let price = data.results[i].fare.total_price;
        let departure = offer[0].outbound.flights[0].origin.airport;
        let arrival = offer[0].inbound.flights[0].origin.airport;
        let depDate = moment(offer[0].outbound.flights[0].departs_at).format('DD.MM.YYYY u HH:mm');
        let arrDate = moment(offer[0].inbound.flights[0].departs_at).format('DD.MM.YYYY u HH:mm');
        let depConn = offer[0].outbound.flights.length;
        let arrConn = offer[0].inbound.flights.length;
        let free = offer[0].inbound.flights[0].booking_info.seats_remaining;

        // Prikazujemo ih
        $('#table-panel tbody').append('' +
            '<tr>' +
            '<td class="text-center">#' + ++count + '</td>' +
            '<td><b>POLAZAK</b><br /><b>POVRATAK</b></td>' +
            '<td>' + departure + '<br />' + arrival + '</td>' +
            '<td>' + depDate + '<br />' + arrDate + '</td>' +
            '<td class="text-center">' + depConn + '<br />' + arrConn + '</td>' +
            '<td class="text-center">' + free + '</td>' +
            '<td>' + price + ' ' + currency + '</td>' +
            '</tr>'
        );
    }
}
//-----------------------------------------
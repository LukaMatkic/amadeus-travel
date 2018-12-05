USE [amadeus-travel]
GO
/****** Object:  Table [dbo].[airport]    Script Date: 5.12.2018. 0:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[airport](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[iata] [varchar](3) NOT NULL,
	[name] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_airport] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_airport] UNIQUE NONCLUSTERED 
(
	[iata] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[result]    Script Date: 5.12.2018. 0:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[result](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[departure] [nchar](3) NOT NULL,
	[arrival] [nchar](3) NOT NULL,
	[dep_date] [datetime] NOT NULL,
	[arr_date] [datetime] NOT NULL,
	[passangers] [int] NOT NULL,
	[currency] [nchar](3) NOT NULL,
	[json_result] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_result] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

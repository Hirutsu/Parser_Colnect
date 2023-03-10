USE [master]
GO
/****** Object:  Database [ParserColnect]    Script Date: 12/25/2022 10:56:51 PM ******/
CREATE DATABASE [ParserColnect]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ParserColnect', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\DATA\ParserColnect.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'ParserColnect_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\DATA\ParserColnect_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [ParserColnect] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ParserColnect].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ParserColnect] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ParserColnect] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ParserColnect] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ParserColnect] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ParserColnect] SET ARITHABORT OFF 
GO
ALTER DATABASE [ParserColnect] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ParserColnect] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ParserColnect] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ParserColnect] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ParserColnect] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ParserColnect] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ParserColnect] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ParserColnect] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ParserColnect] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ParserColnect] SET  DISABLE_BROKER 
GO
ALTER DATABASE [ParserColnect] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ParserColnect] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ParserColnect] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ParserColnect] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ParserColnect] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ParserColnect] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ParserColnect] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ParserColnect] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ParserColnect] SET  MULTI_USER 
GO
ALTER DATABASE [ParserColnect] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ParserColnect] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ParserColnect] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ParserColnect] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [ParserColnect] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [ParserColnect] SET QUERY_STORE = OFF
GO
USE [ParserColnect]
GO
/****** Object:  Table [dbo].[DirtyCoin]    Script Date: 12/25/2022 10:56:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DirtyCoin](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Issuer] [nvarchar](max) NULL,
	[CatalogCode] [nvarchar](max) NULL,
	[Seria] [nvarchar](max) NULL,
	[Topics] [nvarchar](max) NULL,
	[ReleaseDate] [nvarchar](max) NULL,
	[LastReleaseDate] [nvarchar](max) NULL,
	[Spreading] [nvarchar](max) NULL,
	[Thickness] [nvarchar](max) NULL,
	[Denomination] [nvarchar](max) NULL,
	[Mint] [nvarchar](max) NULL,
	[Material] [nvarchar](max) NULL,
	[Gurt] [nvarchar](max) NULL,
	[Shape] [nvarchar](max) NULL,
	[Weight] [nvarchar](max) NULL,
	[Size] [nvarchar](max) NULL,
	[KnownCirculation] [nvarchar](max) NULL,
	[MaterialDetails] [nvarchar](max) NULL,
	[Mark] [nvarchar](max) NULL,
	[SimilarMark] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Orientation] [nvarchar](max) NULL,
	[Kant] [nvarchar](max) NULL,
	[ImgFrontUrl] [nvarchar](max) NULL,
	[ImgBackUrl] [nvarchar](max) NULL,
 CONSTRAINT [PK_DirtyCoin] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
USE [master]
GO
ALTER DATABASE [ParserColnect] SET  READ_WRITE 
GO

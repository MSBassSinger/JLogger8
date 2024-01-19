/*****************************************************************************************************************************
When       Who What
========== === ================================================================================================================
10/01/2019 JDJ Genesis


******************************************************************************************************************************/

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF NOT EXISTS(SELECT name FROM dbo.sysobjects WHERE name = 'DBLog')
	BEGIN

		CREATE TABLE [dbo].[DBLog](
			[ID] [bigint] IDENTITY(1,1) NOT NULL,
			[LogType] [varchar](50) NOT NULL,
			[LogDateTime] [datetime] NOT NULL,
			[LogMessage] [nvarchar](max) NOT NULL,
			[DetailMessage] [nvarchar](max) NOT NULL,
			[ModuleName] [nvarchar](max) NOT NULL,
			[MethodName] [nvarchar](max) NOT NULL,
			[LineNumber] [int] NOT NULL,
			[ThreadID] [int] NOT NULL,
			[ExceptionData] [nvarchar](max) NOT NULL,
			[StackData] [nvarchar](max) NOT NULL,
		 CONSTRAINT [PK_DBLog] PRIMARY KEY CLUSTERED 
		(
			[ID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

		ALTER TABLE [dbo].[DBLog] ADD  CONSTRAINT [DF_DBLog_LogType]  DEFAULT ('Unspecified') FOR [LogType]

		ALTER TABLE [dbo].[DBLog] ADD  CONSTRAINT [DF_DBLog_LogMessage]  DEFAULT ('') FOR [LogMessage]

		ALTER TABLE [dbo].[DBLog] ADD  CONSTRAINT [DF_DBLog_DetailMessage]  DEFAULT ('') FOR [DetailMessage]

		ALTER TABLE [dbo].[DBLog] ADD  CONSTRAINT [DF_DBLog_ModuleName]  DEFAULT ('') FOR [ModuleName]

		ALTER TABLE [dbo].[DBLog] ADD  CONSTRAINT [DF_DBLog_MethodName]  DEFAULT ('') FOR [MethodName]

		ALTER TABLE [dbo].[DBLog] ADD  CONSTRAINT [DF_DBLog_LineNumber]  DEFAULT ((0)) FOR [LineNumber]

		ALTER TABLE [dbo].[DBLog] ADD  CONSTRAINT [DF_DBLog_ThreadID]  DEFAULT ((0)) FOR [ThreadID]

		ALTER TABLE [dbo].[DBLog] ADD  CONSTRAINT [DF_DBLog_ExceptionData]  DEFAULT ('') FOR [ExceptionData]

		ALTER TABLE [dbo].[DBLog] ADD  CONSTRAINT [DF_DBLog_StackData]  DEFAULT ('') FOR [StackData]

	END


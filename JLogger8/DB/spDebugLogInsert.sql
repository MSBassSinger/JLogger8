/******************************************************************************

Object:

   spDebugLogInsert

Description:

   Inserts a new debug log record.


Revision History:
----------------------
10-01-2019	JDJ	Creation.

*******************************************************************************/

IF EXISTS (SELECT name FROM dbo.sysobjects WHERE name = 'spDebugLogInsert')
	DROP PROCEDURE spDebugLogInsert
	
GO 

CREATE PROCEDURE [dbo].[spDebugLogInsert]
	(
	@ID					bigint OUTPUT, 
	@LogType			varchar(50), 
	@LogDateTime		datetime, 
	@LogMessage			nvarchar(MAX), 
	@DetailMessage		nvarchar(MAX), 
	@ModuleName			nvarchar(MAX), 
	@MethodName			nvarchar(MAX), 
	@LineNumber			int, 
	@ThreadID			int, 
	@ExceptionData		nvarchar(MAX), 
	@StackData			nvarchar(MAX), 
	@RowsAffected       INT = 0 OUTPUT,
	@ErrMessage         NVARCHAR(255) = '' OUTPUT	)
AS
BEGIN

	/*
	When SET NOCOUNT is ON, the count is not returned. When SET NOCOUNT is OFF, the count is returned.
	The @@ROWCOUNT function is updated even when SET NOCOUNT is ON.
	SET NOCOUNT ON prevents the sending of DONE_IN_PROC messages to the client for each statement 
	in a stored procedure. For stored procedures that contain several statements that do not return much 
	actual data, setting SET NOCOUNT to ON can provide a significant performance boost, because 
	network traffic is greatly reduced.
	The setting specified by SET NOCOUNT is in effect at execute or run time and not at parse time.
	*/
	SET NOCOUNT ON

	DECLARE @ErrNum	INT
	DECLARE @NumRows BIGINT
	DECLARE @throwMsg varchar(2048)
	SET @ErrNum = 0
	SET @ErrMessage = ''


	BEGIN TRY
		
		-- Insert the record
		INSERT INTO [dbo].[DBLog] 
		( 
			[LogType], 
			[LogDateTime], 
			[LogMessage], 
			[DetailMessage], 
			[ModuleName], 
			[MethodName], 
			[LineNumber], 
			[ThreadID], 
			[ExceptionData], 
			[StackData]
		)
		VALUES
		(
			@LogType, 
			@LogDateTime, 
			@LogMessage, 
			@DetailMessage, 
			@ModuleName, 
			@MethodName, 
			@LineNumber, 
			@ThreadID, 
			@ExceptionData, 
			@StackData
		)

		SET @ID = SCOPE_IDENTITY();

		SET @NumRows = @@ROWCOUNT;

		SET @RowsAffected = @NumRows;

		IF @NumRows = 0 
			BEGIN;
				SET @ErrNum = 50004;
				SET @throwMsg = 'The attempt to insert the debug log record failed.';
				THROW @ErrNum, @throwMsg, 1;
			END;

	END TRY
	BEGIN CATCH
		BEGIN
			-- Capture the error number.
			SET @ErrNum = @@ERROR;
			SET @ErrMessage = 'Error#=[' + CONVERT(nvarchar(255), @ErrNum) + ']::Line=[' + CONVERT(nvarchar(255), ERROR_LINE()) + ']::Message=[' + COALESCE(ERROR_MESSAGE(), 'N/A') + ']::=Procedure=[' + COALESCE(ERROR_PROCEDURE(), 'N/A') + ']::Severity=[' + CONVERT(nvarchar(255), ERROR_SEVERITY()) + ']::Application=[' + APP_NAME() + ']';
		END
	END CATCH

	RETURN @ErrNum

END


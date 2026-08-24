CREATE TABLE [dbo].[DoorState]
(
	DoorStateId INT NOT NULL 
	CONSTRAINT PK_DoorState_DoorStateId PRIMARY KEY CLUSTERED,
	DoorStateDescription VARCHAR(100)
)

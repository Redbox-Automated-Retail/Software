CREATE TABLE Queue(
    ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Priority tinyint NOT NULL,
    Type varchar(32) NOT NULL,
    Data image NOT NULL,
    CreatedOn datetime NOT NULL DEFAULT 'GETDATE()' );
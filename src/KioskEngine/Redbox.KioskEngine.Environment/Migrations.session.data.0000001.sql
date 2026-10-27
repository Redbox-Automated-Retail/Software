CREATE TABLE Session(
    ID uniqueidentifier NOT NULL PRIMARY KEY,
    StoreNumber varchar(32) NOT NULL,
    StartedOn datetime NOT NULL,
    EndedOn datetime NULL,
    Status tinyint NOT NULL );
    
CREATE TABLE SessionEvent(
    ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SessionID uniqueidentifier NOT NULL,
    Type tinyint NOT NULL,
    Description varchar(1024) NULL,
    CreatedOn datetime NOT NULL DEFAULT 'GETDATE()',
    CONSTRAINT FK_SessionEvent_Session FOREIGN KEY(SessionID) REFERENCES Session(ID) ON DELETE CASCADE);
    
CREATE TABLE ShoppingCart(
    ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SessionID uniqueidentifier NOT NULL,
    Type tinyint NOT NULL,
    LastName varchar(16) NULL,
    FirstName varchar(16) NULL,
    LastFour char(4) NULL,
    ExpiryMonth char(2) NULL,
    ExpiryYear char(2) NULL,
    PostalCode varchar(16) NULL,
    Email varchar(128) NULL,
    DiscountCode varchar(64) NULL,
    DiscountValue smallmoney NOT NULL DEFAULT '0.00',
    ReferenceNumber varchar(64) NULL,
    CreatedOn datetime NOT NULL DEFAULT 'GETDATE()',
    CONSTRAINT FK_ShoppingCart_Session FOREIGN KEY(SessionID) REFERENCES Session(ID) ON DELETE CASCADE);
    
CREATE TABLE ShoppingCartLineItemGroup(
    ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,                
    ShoppingCartID int NOT NULL,
    Name varchar(32) NOT NULL,
    TaxRate smallmoney NOT NULL DEFAULT '0.00',
    SubTotal money NOT NULL DEFAULT '0.00',
    TaxAmount money NOT NULL DEFAULT '0.00',
    GrandTotal money NOT NULL DEFAULT '0.00',
    DiscountedSubTotal money NOT NULL DEFAULT '0.00',
    CONSTRAINT FK_ShoppingCartLineItemGroup_ShoppingCart FOREIGN KEY(ShoppingCartID) REFERENCES ShoppingCart(ID) ON DELETE CASCADE );
    
CREATE TABLE ShoppingCartLineItem(
    ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ShoppingCartLineItemGroupID int NOT NULL,
    Barcode varchar(64) NOT NULL,
    Status tinyint NOT NULL,
    Price smallmoney NOT NULL DEFAULT '0.00',
    Quantity decimal NOT NULL DEFAULT '0',
    CONSTRAINT FK_ShoppingCartLineItem_ShoppingCartLineItemGroup FOREIGN KEY(ShoppingCartLineItemGroupID) REFERENCES ShoppingCartLineItemGroup(ID) ON DELETE CASCADE);
    
CREATE TABLE ShoppingCartLineItemPrice(
    ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ShoppingCartLineItemID int NOT NULL,
    Name varchar(64) NOT NULL,
    Value smallmoney NOT NULL DEFAULT '0.00',
    CONSTRAINT FK_ShoppingCartLineItemPrice_ShoppingCartLineItem FOREIGN KEY(ShoppingCartLineItemID) REFERENCES ShoppingCartLineItem(ID) ON DELETE CASCADE );
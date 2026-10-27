namespace Redbox.BrokerServices.Proxy.ComponentModel;

public delegate ILocalCancelReservationResult CancelReservationCallback(long referenceNumber, bool cancelOnServer = true);

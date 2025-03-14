﻿namespace DesolaServices.DataTransferObjects.Responses;

public class FlightItineraryGroupResponse
{
    public decimal TotalPrice { get; set; }
    public string PriceCurrency { get; set; }
    public FlightItineraryResponse Departure { get; set; }
    public FlightItineraryResponse Return { get; set; }

}

public class FlightItineraryResponse
{
    public string TotalDuration { get; set; }
    public int NumberOfStopOver { get; set; }
    public List<FlightSegmentResponse> Segments { get; set; } = new();
}

public class FlightItineraryLegResponse
{
    public string Id { get; set; }

    public string Origin { get; set; }

    public string Destination { get; set; }
}


public class FlightSegmentResponse
{
    public string FlightFrom { get; set; }
    public string FlightTo { get; set; }
    public string DepartureDateTime { get; set; }
    public string ArrivalDateTime { get; set; }
    public string FlightNumber { get; set; }
    public string Airline { get; set; }
    public string Aircraft { get; set; }
    public string AircraftPhotoLink { get; set; }
    public string FlightDuration { get; set; }
}

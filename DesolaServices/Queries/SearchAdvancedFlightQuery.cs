﻿using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Requests;
using MediatR;

namespace DesolaServices.Queries;

public class SearchAdvancedFlightQuery (FlightSearchAdvanced criteria): IRequest<amadeus.resources.FlightOffer[]>
{
    public FlightSearchAdvanced Criteria { get; } = criteria;
}
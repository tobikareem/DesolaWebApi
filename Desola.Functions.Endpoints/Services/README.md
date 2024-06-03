# Org.OpenAPITools - Azure Functions v4 Server

Welcome to the Amadeus for Developers collection. This collection features a list of [Amadeus Self-Service APIs](https://developers.amadeus.com/self-service), categorized for your convenience and learning.

The Self-Service catalog is divided into six categories:

- [<b>Flight APIs</b>](https://developers.amadeus.com/self-service/category/flights) (flight search, flight booking and airport information)
- [<b>Destination Experiences APIs</b>](https://developers.amadeus.com/self-service/category/destination-experiences) (tours and activities, tourist attractions and safety data)
- [<b>Cars and Transfers APIs</b>](https://developers.amadeus.com/self-service/category/cars-and-transfers) (transfer search, booking and management)
- [<b>Market Insights</b>](https://developers.amadeus.com/self-service/category/market-insights) (most booked and traveled destinations, plus location scores)
- [<b>Hotel APIs</b>](https://developers.amadeus.com/self-service/category/hotels) (hotel search, booking and ratings)
- [<b>Itinerary Management APIs</b>](https://developers.amadeus.com/self-service/category/itinerary-management) (trip-planning functionalities)
    

## 🛟 Help and support

If you have any questions of Amadeus Self-Service APIs, please have a look at [Amadeus for Developers Documentation](https://developers.amadeus.com/self-service/apis-docs/guides/developer-guides/) or check out our [discord channel ](https://discord.gg/cVrFBqx) to join the community!

## Run

Linux/OS X:

```
sh build.sh
```

Windows:

```
build.bat
```
## Run in Docker

```
cd src/Org.OpenAPITools
docker build -t org.openapitools .
docker run -p 5000:8080 org.openapitools
```

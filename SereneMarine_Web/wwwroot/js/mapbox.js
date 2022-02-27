/**
 * @name CreateMapBox
 * @description Initializes the mapbox required for mapboxgl
 *
 * @returns {object} map
 */
function CreateMapBox() {
    var map = new mapboxgl.Map({
        container: 'map',
        style: 'mapbox://styles/mapbox/streets-v11',
        center: [31.0218, -29.8587], // starting position [lng, lat]
        zoom: 12 // starting zoom

    });

    map.addControl(
        new MapboxGeocoder({
            accessToken: mapboxgl.accessToken,
            mapboxgl: mapboxgl
        })
    );

    return map;
}

function AddSingleMarkerToMapByLatAndLongCoordinate(mapbox, lat, long, eventName) {
    var latC = ReplaceDecimalWithPeriod(lat);
    var longC = ReplaceDecimalWithPeriod(long);

    window["Marker"] = new mapboxgl.Marker().setLngLat([latC, longC]).addTo(mapbox).setPopup(new mapboxgl.Popup().setHTML(eventName));
}

function AddMarkersToMapByLatAndLongArrays(mapbox, latArray, longArray, eventNameArray) {
    for (var i = 0; i < latArray.length; i++) {

        var latC = ReplaceDecimalWithPeriod(latArray[i]);
        var longC = ReplaceDecimalWithPeriod(longArray[i]);

        window["Marker" + i] = new mapboxgl.Marker().setLngLat([latC, longC]).addTo(mapbox).setPopup(new mapboxgl.Popup().setHTML(eventNameArray[i]));
    }
}

function ShowOnMap(x) {
    var point = window["Marker" + x]
    point.togglePopup();
}

function ReplaceDecimalWithPeriod(value) {
    return value.toString().replace(/,/, '.')
}
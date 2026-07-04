const PharmacyMapIcons = {
    pharmacy: L.divIcon({
        className: 'pharmacy-marker-icon',
        html: '<div class="pharmacy-pin"><span>+</span></div>',
        iconSize: [34, 34],
        iconAnchor: [17, 34]
    }),
    user: L.divIcon({
        className: 'user-marker-icon',
        html: '<div class="user-pin"></div>',
        iconSize: [22, 22],
        iconAnchor: [11, 11]
    })
};

// Get user location
function getUserLocation() {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject(new Error('Geolocation not supported'));
            return;
        }

        navigator.geolocation.getCurrentPosition(
            pos => resolve({
                lat: pos.coords.latitude,
                lng: pos.coords.longitude
            }),
            err => reject(err),
            { enableHighAccuracy: true, timeout: 10000 }
        );
    });
}

// 🟢 OSM Overpass API (NO BACKEND)
async function fetchNearbyPharmacies({ lat, lng, radius = 3000 } = {}) {
    if (!lat || !lng) return [];

    const query = `
    [out:json];
    (
      node["amenity"="pharmacy"](around:${radius},${lat},${lng});
      way["amenity"="pharmacy"](around:${radius},${lat},${lng});
    );
    out center;
    `;

    const url = "https://overpass-api.de/api/interpreter?data=" + encodeURIComponent(query);

    const res = await fetch(url);
    const data = await res.json();

    return (data.elements || []).map(el => ({
        id: el.id,

        // 🟢 FIXED NAME LOGIC
        name:
            el.tags?.name ||
            el.tags?.["name:en"] ||
            el.tags?.["name:ar"] ||
            "Pharmacy",

        latitude: el.lat ?? el.center?.lat,
        longitude: el.lon ?? el.center?.lon,

        address:
            el.tags?.["addr:street"] ||
            el.tags?.["addr:full"] ||
            el.tags?.["addr:city"] ||
            "",

        phoneNumber:
            el.tags?.phone ||
            el.tags?.["contact:phone"] ||
            "",

        openingHours:
            el.tags?.["opening_hours"] || ""
    }));
}

// Map init
function createPharmacyMap(elementId, center = [30.0444, 31.2357], zoom = 13) {
    const map = L.map(elementId).setView(center, zoom);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);

    return map;
}

// Render markers
function renderMarkers(map, pharmacies, { userCoords, onMarkerClick } = {}) {
    const markers = {};
    const bounds = [];

    if (userCoords) {
        L.marker([userCoords.lat, userCoords.lng], { icon: PharmacyMapIcons.user })
            .addTo(map)
            .bindTooltip('You', { permanent: true, direction: 'top' });

        bounds.push([userCoords.lat, userCoords.lng]);
    }

    pharmacies.forEach(p => {
        const marker = L.marker([p.latitude, p.longitude], {
            icon: PharmacyMapIcons.pharmacy
        })
            .addTo(map)
            .bindTooltip(p.name, {
                permanent: true,
                direction: 'top'
            });

        marker.on('click', () => onMarkerClick?.(p.id));

        markers[p.id] = marker;
        bounds.push([p.latitude, p.longitude]);
    });

    if (bounds.length) {
        map.fitBounds(bounds, { padding: [40, 40], maxZoom: 15 });
    }

    return markers;
}
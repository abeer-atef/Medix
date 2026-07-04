(function () {

    let map;
    let markers = [];
    let userMarker;

    document.addEventListener("DOMContentLoaded", () => {
        initMap();
        bindButton();
    });

    function initMap() {
        const el = document.getElementById("homePharmacyMap");
        if (!el) return;

        map = L.map(el).setView([30.0444, 31.2357], 6);

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: "© OpenStreetMap"
        }).addTo(map);
    }

    function bindButton() {
        const btn = document.getElementById("detectLocationBtn");
        if (btn) btn.addEventListener("click", detectLocation);
    }

    function detectLocation() {
        navigator.geolocation.getCurrentPosition(
            async (pos) => {
                const lat = pos.coords.latitude;
                const lng = pos.coords.longitude;

                map.setView([lat, lng], 15);

                if (userMarker) userMarker.remove();

                userMarker = L.marker([lat, lng])
                    .addTo(map)
                    .bindPopup("You are here")
                    .openPopup();

                const pharmacies = await fetchNearbyPharmacies({ lat, lng });
                render(pharmacies, lat, lng);
            },
            () => alert("Location denied")
        );
    }

    function render(pharmacies, userLat, userLng) {

        markers.forEach(m => m.remove());
        markers = [];

        const list = document.getElementById("homePharmacyList");
        if (!list) return;

        if (!pharmacies.length) {
            list.innerHTML = "<li>No pharmacies found</li>";
            return;
        }

        list.innerHTML = "";

        pharmacies.forEach(p => {

            const dist = haversine(userLat, userLng, p.latitude, p.longitude);

            const li = document.createElement("li");
            li.innerHTML = `
                <strong>${p.name}</strong>
                <span>${(dist / 1000).toFixed(1)} km</span>
            `;

            const marker = L.marker([p.latitude, p.longitude])
                .addTo(map)
                .bindPopup(`
                    <b>${p.name}</b><br>
                    ${p.address || ''}<br>
                    ${p.phoneNumber || ''}
                `);

            li.onclick = () => {
                map.setView([p.latitude, p.longitude], 17);
                marker.openPopup();
            };

            list.appendChild(li);
            markers.push(marker);
        });
    }

    function haversine(lat1, lng1, lat2, lng2) {
        const R = 6371000;
        const dLat = (lat2 - lat1) * Math.PI / 180;
        const dLng = (lng2 - lng1) * Math.PI / 180;

        const a =
            Math.sin(dLat / 2) ** 2 +
            Math.cos(lat1 * Math.PI / 180) *
            Math.cos(lat2 * Math.PI / 180) *
            Math.sin(dLng / 2) ** 2;

        return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    }

})();
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
                render(pharmacies);
            },
            () => alert("Location denied")
        );
    }

    function render(pharmacies) {

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
            const li = document.createElement("li");
            li.innerHTML = `
                <strong>${p.name}</strong>
                <span>${p.distanceKm.toFixed(1)} km</span>
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

})();
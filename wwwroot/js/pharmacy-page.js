document.addEventListener('DOMContentLoaded', () => {
    const map = createPharmacyMap('pharmacyMap');
    const listEl = document.getElementById('pharmacyList');
    const searchInput = document.getElementById('pharmacySearch');
    const detectBtn = document.getElementById('detectLocationBtn');

    let userCoords = null;
    let activeMarkers = {};
    let debounce = null;

    function renderList(pharmacies) {
        listEl.innerHTML = '';

        if (!pharmacies.length) {
            listEl.innerHTML = '<p class="pharmacy-empty">No pharmacies found.</p>';
            return;
        }

        pharmacies.forEach(p => {
            const item = document.createElement('div');
            item.className = 'pharmacy-card';

            item.innerHTML = `
                <h4>${p.name} ${p.distanceKm != null ? `<span class="dist-badge">${p.distanceKm.toFixed(1)} km</span>` : ''}</h4>
                <p>${p.address || 'No address available'}</p>
                ${p.phoneNumber ? `<a href="tel:${p.phoneNumber}">${p.phoneNumber}</a>` : ''}
                ${p.openingHours ? `<small>🕐 ${p.openingHours}</small>` : ''}
            `;

            item.addEventListener('click', () => focusPharmacy(p.id));
            listEl.appendChild(item);
        });
    }

    function focusPharmacy(id) {
        const marker = activeMarkers[id];
        if (marker) {
            map.setView(marker.getLatLng(), 16);
            marker.openTooltip();
        }
    }

    async function loadPharmacies() {
        try {
            const pharmacies = await fetchNearbyPharmacies({
                lat: userCoords?.lat,
                lng: userCoords?.lng
            });

            renderList(pharmacies);

            Object.values(activeMarkers).forEach(m => map.removeLayer(m));

            activeMarkers = renderMarkers(map, pharmacies, {
                userCoords,
                onMarkerClick: focusPharmacy
            });

        } catch (err) {
            console.error(err);
            listEl.innerHTML = '<p class="pharmacy-empty">Error loading pharmacies</p>';
        }
    }


    detectBtn.addEventListener('click', async () => {
        detectBtn.disabled = true;

        try {
            userCoords = await getUserLocation();
            await loadPharmacies();
        } catch {
            alert("Please allow location access");
        }

        detectBtn.disabled = false;
    });

    searchInput?.addEventListener('input', () => {
        clearTimeout(debounce);
        debounce = setTimeout(loadPharmacies, 300);
    });

    loadPharmacies();
});
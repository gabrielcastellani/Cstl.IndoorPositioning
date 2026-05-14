window.LeafletInterop = (() => {
    let map = null;
    let beaconMarkers = {};
    let deviceMarker  = null;
    let accuracyCircle = null;
    let dotNetRef     = null;

    function beaconIcon(mac, active) {
        const color = active ? '#3b82f6' : '#475569';
        const html = `
            <div style="
                width:36px;height:36px;border-radius:50%;
                background:${color}22;border:2px solid ${color};
                display:flex;align-items:center;justify-content:center;
                font-size:16px;cursor:move;box-shadow:0 0 12px ${color}55;">
                📡
            </div>`;
        return L.divIcon({ html, className: '', iconSize: [36, 36], iconAnchor: [18, 18] });
    }

    function deviceIcon() {
        const html = `
            <div style="
                width:20px;height:20px;border-radius:50%;
                background:#3b82f6;border:3px solid #fff;
                box-shadow:0 0 0 4px #3b82f644,0 2px 8px #0008;">
            </div>`;
        return L.divIcon({ html, className: '', iconSize: [20, 20], iconAnchor: [10, 10] });
    }

    function init(elementId, lat, lon, zoom, dotNetObjRef) {
        if (map) {
            map.remove();
            map = null;
            beaconMarkers = {};
            deviceMarker = null;
        }

        dotNetRef = dotNetObjRef;

        map = L.map(elementId, { zoomControl: true, attributionControl: false }).setView([lat, lon], zoom);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 22
        }).addTo(map);

        map.on('click', (e) => {
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnMapClick', e.latlng.lat, e.latlng.lng);
            }
        });

        return true;
    }

    function setView(lat, lon, zoom) {
        if (!map) return;
        map.setView([lat, lon], zoom ?? map.getZoom());
    }

    function upsertBeacon(mac, lat, lon, label, active) {
        if (!map) return;

        if (beaconMarkers[mac]) {
            beaconMarkers[mac].setLatLng([lat, lon]);
            beaconMarkers[mac].setIcon(beaconIcon(mac, active));
            beaconMarkers[mac].getPopup()?.setContent(popupHtml(mac, label, lat, lon));
            return;
        }

        const marker = L.marker([lat, lon], {
            icon: beaconIcon(mac, active),
            draggable: true
        }).addTo(map);

        marker.bindPopup(popupHtml(mac, label, lat, lon));

        marker.on('dragend', () => {
            const pos = marker.getLatLng();
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnBeaconDragged', mac, pos.lat, pos.lng);
            }
            marker.getPopup()?.setContent(popupHtml(mac, label, pos.lat, pos.lng));
        });

        marker.on('click', () => {
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnBeaconClicked', mac);
            }
        });

        beaconMarkers[mac] = marker;
    }

    function removeBeacon(mac) {
        if (beaconMarkers[mac]) {
            map.removeLayer(beaconMarkers[mac]);
            delete beaconMarkers[mac];
        }
    }

    function clearBeacons() {
        Object.keys(beaconMarkers).forEach(removeBeacon);
    }

    function setDevicePosition(lat, lon, accuracyMeters) {
        if (!map) return;

        if (!deviceMarker) {
            deviceMarker = L.marker([lat, lon], { icon: deviceIcon(), zIndexOffset: 1000 }).addTo(map);
            deviceMarker.bindPopup('<b>Estimated position</b>');
        } else {
            deviceMarker.setLatLng([lat, lon]);
        }

        if (accuracyCircle) map.removeLayer(accuracyCircle);
        if (accuracyMeters > 0) {
            accuracyCircle = L.circle([lat, lon], {
                radius: accuracyMeters,
                color: '#3b82f6',
                fillColor: '#3b82f622',
                fillOpacity: 1,
                weight: 1,
                dashArray: '4'
            }).addTo(map);
        }
    }

    function clearDevicePosition() {
        if (deviceMarker) { map.removeLayer(deviceMarker); deviceMarker = null; }
        if (accuracyCircle) { map.removeLayer(accuracyCircle); accuracyCircle = null; }
    }

    function panToDevice(lat, lon) {
        if (!map) return;
       
        const bounds = map.getBounds();
        if (!bounds.contains([lat, lon])) {
            map.panTo([lat, lon], { animate: true, duration: 0.5 });
        }
    }

    function popupHtml(mac, label, lat, lon) {
        return `<div style="font-family:Poppins,sans-serif;min-width:160px">
            <div style="font-weight:600;margin-bottom:4px">${label || mac}</div>
            <div style="font-size:11px;color:#94a3b8">${mac}</div>
            <div style="font-size:11px;color:#94a3b8;margin-top:4px">
                ${lat.toFixed(6)}, ${lon.toFixed(6)}
            </div>
        </div>`;
    }

    function invalidateSize() {
        if (map) map.invalidateSize();
    }

    return { init, setView, upsertBeacon, removeBeacon, clearBeacons, setDevicePosition, clearDevicePosition, panToDevice, invalidateSize };
})();

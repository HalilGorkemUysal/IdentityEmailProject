// @ts-nocheck
// Dashboard grafikleri (Chart.js). Veriler Views/Dashboard/Index.cshtml içindeki
// <script type="application/json" id="dashboardData"> etiketinden okunur.
(() => {
    const dataEl = document.getElementById('dashboardData');
    if (!dataEl || typeof Chart === 'undefined') return;

    // { labels, received, sent, categories: [{ name, count, color }, ...] }
    const data = JSON.parse(dataEl.textContent);
    const daily = { labels: data.labels, received: data.received, sent: data.sent };
    const categories = data.categories;

    const MUTED = '#85899B', GRID = '#F0EFF7', SURFACE = '#ffffff';
    const COLOR_RECEIVED = '#8B7FD6', COLOR_SENT = '#3E9A77';

    Chart.defaults.font.family = getComputedStyle(document.body).fontFamily;
    Chart.defaults.font.size = 12;
    Chart.defaults.color = MUTED;

    const tooltip = {
        backgroundColor: '#2E3045', titleColor: '#fff', bodyColor: '#fff',
        padding: 10, cornerRadius: 10, boxPadding: 4, usePointStyle: true
    };

    /* ---------- Sütun grafik ---------- */
    const bar = (label, values, color) => ({
        label,
        data: values,
        backgroundColor: color,
        borderRadius: 4,            // sadece üst köşeler yuvarlak
        borderSkipped: 'start',     // alt kenar eksene düz otursun
        maxBarThickness: 22,
        categoryPercentage: 0.6,
        barPercentage: 0.82         // iki sütun arasında küçük boşluk
    });

    const dailyCanvas = document.getElementById('dailyChart');
    if (dailyCanvas) {
        new Chart(dailyCanvas, {
            type: 'bar',
            data: {
                labels: daily.labels,
                datasets: [bar('Alınan', daily.received, COLOR_RECEIVED), bar('Gönderilen', daily.sent, COLOR_SENT)]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },   // gün üzerine gelince ikisini birlikte göster
                plugins: {
                    legend: { display: false },                     // lejant kartın başlığında
                    tooltip: { ...tooltip, callbacks: { label: c => ` ${c.dataset.label}: ${c.parsed.y} mesaj` } }
                },
                scales: {
                    x: { grid: { display: false }, border: { display: false }, ticks: { color: MUTED } },
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0, color: MUTED, maxTicksLimit: 6 },   // 1.5 mesaj olmaz
                        grid: { color: GRID },
                        border: { display: false }
                    }
                }
            }
        });
    }

    /* ---------- Halka grafik ---------- */
    const donut = document.getElementById('categoryChart');
    if (donut && categories.length) {
        const total = categories.reduce((s, c) => s + c.count, 0);

        new Chart(donut, {
            type: 'doughnut',
            data: {
                labels: categories.map(c => c.name),
                datasets: [{
                    data: categories.map(c => c.count),
                    backgroundColor: categories.map(c => c.color),
                    borderColor: SURFACE,   // dilimler arasında 2px beyaz boşluk
                    borderWidth: 2,
                    hoverOffset: 4
                }]
            },
            options: {
                cutout: '70%',
                plugins: {
                    legend: { display: false },   // lejant grafiğin altında HTML liste
                    tooltip: {
                        ...tooltip,
                        callbacks: { label: c => ` ${c.label}: ${c.parsed} mesaj (%${Math.round(c.parsed * 100 / total)})` }
                    }
                }
            }
        });
    }
})();

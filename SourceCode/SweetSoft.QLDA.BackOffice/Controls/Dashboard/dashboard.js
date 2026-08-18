var DashboardJs = {};
DashboardJs.ResourceTexts = {

}

const currentLang = $('html').attr('lang');
const _resourceText = DashboardJs.ResourceTexts[currentLang];

DashboardJs = DashboardJs || {};

DashboardJs.GetChartColorsArray = (selector) => {
    const colorsData = $(selector).attr("data-colors");
    if (!colorsData) return [];

    return JSON.parse(colorsData).map(color => {
        color = color.trim();
        if (!color.startsWith("--")) return color;
        const cssColor = getComputedStyle(document.documentElement).getPropertyValue(color);
        return cssColor.trim() || undefined;
    });
};
DashboardJs.MeetingRoomStatus = () => {
    const serverData = JSON.parse($('[ data-selector="hdfRoomStatusStats"]').val()) || [];
    console.log(serverData);
    var options = {
        chart: {
            type: 'donut',
            height: 350
        },
        labels: ['Tham gia', 'Đi muộn', 'Vắng', 'Trùng lịch họp'],
        series: serverData || [],
        colors: [
            'rgba(40, 167, 69, 0.8)',   // Tham gia
            'rgba(255, 193, 7, 0.8)',   // Đi muộn
            'rgba(108, 117, 125, 0.8)', // Vắng
            'rgba(220, 53, 69, 0.8)'    // Trung lịch họp
        ],
        stroke: {
            colors: ['#fff'],
            width: 2
        },
        legend: {
            position: 'bottom'
        },
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    height: 300
                },
                legend: {
                    position: 'bottom'
                }
            }
        }]
    };

    var chart = new ApexCharts(document.querySelector("#roomStatusChart"), options);
    chart.render();
};

DashboardJs.PopularMeetingTimeStatistics = () => {
    const serverData = JSON.parse($('[ data-selector="hdfMeetingTimeRange"]').val()) || [];
    var options = {
        series: [{
            name: 'Tỷ lệ',
            data: serverData
        }],
        chart: {
            height: 450,
            type: 'bar',
        },
        plotOptions: {
            bar: {
                borderRadius: 10,
                dataLabels: {
                    position: 'top', // top, center, bottom
                },
            }
        },
        dataLabels: {
            enabled: true,
            formatter: function (val) {
                return val + "%";
            },
            offsetY: -20,
            style: {
                fontSize: '12px',
                colors: ["#304758"]
            }
        },

        xaxis: {
            categories: ['7:00','8:00', '9:00', '10:00', '11:00', '13:00', '14:00', '15:00', '16:00', '17:00'],
            position: 'top',
            axisBorder: {
                show: false
            },
            axisTicks: {
                show: false
            },
            crosshairs: {
                fill: {
                    type: 'gradient',
                    gradient: {
                        colorFrom: '#D8E3F0',
                        colorTo: '#BED1E6',
                        stops: [0, 100],
                        opacityFrom: 0.4,
                        opacityTo: 0.5,
                    }
                }
            },
            tooltip: {
                enabled: true,
            }
        },
        yaxis: {
            axisBorder: {
                show: false
            },
            axisTicks: {
                show: false,
            },
            labels: {
                show: false,
                formatter: function (val) {
                    return val + "%";
                }
            }

        }
    };

    var chart = new ApexCharts(document.querySelector("#timeChart"), options);
    chart.render();
};

DashboardJs.WeeklyMeetingStatistics = () => {
    const serverData = JSON.parse($('[ data-selector="hdfWeeklyMeetingStats"]').val()) || {};
    var options = {
        chart: {
            type: 'line',
            height: 400,
            toolbar: { show: false }
        },
        series: [
            {
                name: 'Số cuộc họp',
                type: 'line',
                data: serverData.meetingArr || [],
                color: '#667EEA'   // Xanh tím
            },
            {
                name: 'Số người tham gia',
                type: 'line',
                data: serverData.participantArr || [],
                color: '#43E97B'   // Xanh lá
            }
        ],
        stroke: {
            curve: 'smooth',
            width: 3
        },
        fill: {
            type: 'gradient',
            gradient: {
                shadeIntensity: 1,
                type: "vertical",
                opacityFrom: 0.4,
                opacityTo: 0.1,
                stops: [0, 100]
            }
        },
        colors: ['#667EEA', '#43E97B'],
        labels: ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'],
        xaxis: {
            categories: ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật']
        },
        yaxis: [
            {
                title: {
                    text: 'Số cuộc họp'
                }
            },
            {
                opposite: true,
                title: {
                    text: 'Số người tham gia'
                }
            }
        ],
        legend: {
            position: 'top'
        }
    };

    var chart = new ApexCharts(document.querySelector("#meetingChart"), options);
    chart.render();
};

DashboardJs.Init = () => {
    DashboardJs.MeetingRoomStatus();
    DashboardJs.PopularMeetingTimeStatistics();
    DashboardJs.WeeklyMeetingStatistics();
};

$(function () {
    DashboardJs.Init();
});

function animateNumberByDataAttr(element) {
    const target = parseInt(element.getAttribute('data-animateNumber'), 10);
    if (isNaN(target)) return;

    let current = 0;
    const increment = target / 50;

    const timer = setInterval(() => {
        current += increment;
        if (current >= target) {
            element.textContent = target.toLocaleString();
            clearInterval(timer);
        } else {
            element.textContent = Math.floor(current).toLocaleString();
        }
    }, 20);
}

window.addEventListener('load', () => {
    document.querySelectorAll('[data-animateNumber]').forEach(el => animateNumberByDataAttr(el));
});

document.querySelectorAll('.stat-card').forEach(card => {
    card.addEventListener('mouseenter', function () {
        this.style.transform = 'translateY(-5px) scale(1.02)';
    });

    card.addEventListener('mouseleave', function () {
        this.style.transform = 'translateY(0) scale(1)';
    });
});
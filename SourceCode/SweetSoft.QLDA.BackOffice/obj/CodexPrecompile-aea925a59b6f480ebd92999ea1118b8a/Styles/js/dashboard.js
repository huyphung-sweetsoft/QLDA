class DashboardCompetitionList {
    constructor(tableSelector) {
        this.dataBox = $(tableSelector);
        this.oneGroupBox = this.dataBox.find('.head-item-cumstom');
        this.chartRowOne = this.dataBox.find('.chart-row-1');
        this.threeChartGrade = this.dataBox.find('#gradeScoreChart');
        this.threeChartTopClass = this.dataBox.find('#topClassChart');
        this.tableListRound = this.dataBox.find('.table-responsive'); // Chứa danh sách con
        this.table = this.tableListRound.find('table');
        this.tbody = this.tableListRound.find('tbody');
        this.tempData = {
            oneGroup: {
                totalCompetition: 5,
                totalCompetitionNow: 2,
                totalRound: 25,
                totalRoundThisMonth: 12,
            },
            twoGroupStatus: {
                labels: ['Chưa bắt đầu', 'Đang diễn ra', 'Hoàn thành'],
                data: [5, 8, 10],
            },
            twoMonthRound: {
                labels: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6', 'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10'],
                data: [2, 3, 1, 4, 2, 3, 0, 1, 4, 4],
            },
            threeGrade: {
                labels: ['Khối 6', 'Khối 7', 'Khối 8', 'Khối 9'],
                data: [200, 180, 76, 300],
            },
            threeTop: {
                labels: ['12A1', '11A3', '12A5', '10A2', '11A1'],
                data: [9.2, 8.9, 8.7, 8.5, 8.3],
            },
            tableCompetition: [
                {
                    "roundName": "Vòng loại khu vực miền Bắc",
                    "className": "Lớp 9A1",
                    "teamName": "Đội Phượng Hoàng",
                    "startDate": "20/10/2025",
                    "viewUrl": "#",
                    "isCompleted": false
                },
                {
                    "roundName": "Vòng loại khu vực miền Trung",
                    "className": "Lớp 8B2",
                    "teamName": "Đội Sao Biển",
                    "startDate": "25/10/2025",
                    "viewUrl": "#",
                    "isCompleted": false
                },
                {
                    "roundName": "Vòng bán kết toàn quốc",
                    "className": "Lớp 9C",
                    "teamName": "Đội Rồng Lửa",
                    "startDate": "15/09/2025",
                    "viewUrl": "#",
                    "isCompleted": true
                },
                {
                    "roundName": "Chung kết toàn quốc",
                    "className": "Lớp 9D",
                    "teamName": "Đội Kim Cương",
                    "startDate": "01/11/2025",
                    "viewUrl": "#",
                    "isCompleted": false
                }
            ]

        };

        this.bindEvents();
    }

    bindEvents() {
        //this.detailDashboardQuestion.on('keypress', 'input', (e) => {
        //    if (e.which === 13) {
        //        e.preventDefault();
        //        const currenttbody = $(e.target).closest('tbody');
        //        this.createNewRow(currenttbody, $(e.target).closest('tr'));
        //    }
        //});

        //this.detailDashboardQuestion.on('click', '.btn-delete-group', (e) => {
        //    e.preventDefault();
        //    this.deleteGroup($(e.target).closest('.itemGroupDetailDashboardQuestion'));
        //});
    }




    importFromJSON(jsonData) {


    }


    // export to json
    exportToJSON(isAllowAddUpdate = true) {

        //$('[data-selector="hdfDetailDashboardCompetition"]').val(JSON.stringify(result));
        //$('[data-selector="btnSaveQuestionHidden"]')[0].click();
    }

    bindOneGroup(objItem) {
        const html = `
            <div class="col-lg-3 col-md-4 col-sm-6 mb-3">
                <div class="summary-card">
                    <div class="icon text-warning">🏆</div>
                    <div class="value">${objItem.totalCompetition}</div>
                    <div class="title">Tổng số cuộc thi</div>
                </div>
            </div>
            <div class="col-lg-3 col-md-4 col-sm-6 mb-3">
                <div class="summary-card">
                    <div class="icon text-success">📅</div>
                    <div class="value">${objItem.totalCompetitionNow}</div>
                    <div class="title">Cuộc thi đang diễn ra</div>
                </div>
            </div>
            <div class="col-lg-3 col-md-4 col-sm-6 mb-3">
                <div class="summary-card">
                    <div class="icon text-primary">🧑‍🎓</div>
                    <div class="value">${objItem.totalRound}</div>
                    <div class="title">Tổng các vòng thi</div>
                </div>
            </div>
            <div class="col-lg-3 col-md-4 col-sm-6 mb-3">
                <div class="summary-card">
                    <div class="icon text-info">🏫</div>
                    <div class="value">${objItem.totalRoundThisMonth}</div>
                    <div class="title">Tổng số vòng thi trong tháng này</div>
                </div>
            </div>
        `
        this.oneGroupBox.html(html);
    }

    bindTwoStatus(objItem) {
        // Chart 1: Competition Status (Donut Chart)
        const statusCtx = document.getElementById('competitionStatusChart').getContext('2d');
        const statusChart = new Chart(statusCtx, {
            type: 'doughnut',
            data: {
                labels: objItem.labels,
                datasets: [{
                    data: objItem.data,
                    backgroundColor: [
                        '#3498db', // blue
                        '#f39c12', // orange
                        '#2ecc71', // green
                    ],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }

    bindTwoMonthRound(objItem) {
        // Chart 2: Competition Trend (Line Chart)
        const trendCtx = document.getElementById('competitionTrendChart').getContext('2d');
        const trendChart = new Chart(trendCtx, {
            type: 'line',
            data: {
                labels: objItem.labels,
                datasets: [{
                    label: 'Số cuộc thi',
                    data: objItem.data,
                    borderColor: '#3498db',
                    backgroundColor: 'rgba(52, 152, 219, 0.1)',
                    tension: 0.3,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            precision: 0
                        }
                    }
                }
            }
        });
    }

    bindThreeGrade(objItem) {
        // Chart 3: Grade Score (Bar Chart)
        const gradeCtx = document.getElementById('gradeScoreChart').getContext('2d');
        const gradeChart = new Chart(gradeCtx, {
            type: 'bar',
            data: {
                labels: objItem.labels,
                datasets: [{
                    label: 'Điểm trung bình',
                    data: objItem.data,
                    backgroundColor: [
                        'rgba(52, 152, 219, 0.7)',
                        'rgba(46, 204, 113, 0.7)',
                        'rgba(243, 156, 18, 0.7)',
                        '#2ecc71',
                    ],
                    borderColor: [
                        'rgba(52, 152, 219, 1)',
                        'rgba(46, 204, 113, 1)',
                        'rgba(243, 156, 18, 1)',
                        '#2ecc71',
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 10
                    }
                }
            }
        });

    }

    bindThreeTop(objItem) {
        // Chart 4: Top Classes (Horizontal Bar Chart)
        const topClassCtx = document.getElementById('topClassChart').getContext('2d');
        const topClassChart = new Chart(topClassCtx, {
            type: 'bar',
            data: {
                labels: objItem.labels,
                datasets: [{
                    label: 'Điểm trung bình',
                    data: objItem.data,
                    backgroundColor: 'rgba(46, 204, 113, 0.7)',
                    borderColor: 'rgba(46, 204, 113, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        beginAtZero: true,
                        max: 10
                    }
                }
            }

        });
    }

    bindTableCompetitionRound(objItem) {
        this.tbody.html('');
        // Chart 4: Top Classes (Horizontal Bar Chart)
        objItem.forEach((row, index) => {
            const html = `
            <tr>
                <td>${index + 1}</td>
                <td>${row.roundName}</td>
                <td>${row.className}</td>
                <td>${row.teamName}</td>
                <td>${row.startDate}</td>
                <td>${this.getCompetitionStatus(row.startDate, row.isCompleted)}</td>
                <td>
                    <a href="${row.viewUrl}" class="btn btn-sm btn-outline-primary"><i class="fas fa-eye"></i></a>
                </td>
            </tr>`;
            this.tbody.append(html);
        });
        

    }

    getCompetitionStatus(startDate, isCompleted) {
        if (isCompleted) {
            return `<span class="status-badge status-completed">Hoàn thành</span`;
        }

        const today = new Date();
        const start = new Date(startDate);
        const todayDateOnly = new Date(today.getFullYear(), today.getMonth(), today.getDate());
        const startDateOnly = new Date(start.getFullYear(), start.getMonth(), start.getDate());

        if (startDateOnly.getTime() > todayDateOnly.getTime()) {
            return `<span class="status-badge status-notstarted">Chưa bắt đầu</span>`;
        } else {
            return `<span class="status-badge status-inprogress">Đang diễn ra</span>`;
        }
    }

    showToast(message, title = '', type = 'warning') {
        if (!window.toastr) {
            alert(message);
            return;
        }
        switch (type) {
            case 'success':
                toastr.success(message, title, { timeOut: 3000, closeButton: true, progressBar: true });
                break;
            case 'error':
                toastr.error(message, title, { timeOut: 3000, closeButton: true, progressBar: true });
                break;
            case 'warning':
                toastr.warning(message, title, { timeOut: 3000, closeButton: true, progressBar: true });
                break;
            case 'info':
            default:
                toastr.info(message, title, { timeOut: 3000, closeButton: true, progressBar: true });
                break;
        }
        return true;
    }

    rebind() {
        const startData = this.tempData;
        if (startData) {
            this.bindOneGroup(startData.oneGroup)
            this.bindTwoStatus(startData.twoGroupStatus)
            this.bindTwoMonthRound(startData.twoMonthRound)
            this.bindThreeGrade(startData.threeGrade)
            this.bindThreeTop(startData.threeTop)
            this.bindTableCompetitionRound(startData.tableCompetition)

        }

        //const startData = $('[data-selector="hdfDetailDashboardCompetition"]').val();
        //if (typeof startData === 'string' && startData.trim() !== '') {
        //    const parsedData = JSON.parse(startData);
        //    if (parsedData && parsedData.ListGroup) {
        //        this.importFromJSON(parsedData);
        //    }
        //    else
        //        console.error('Invalid data');
        //}
        //else
        //    console.warn('Invalid data');
    }
}

var dashboardCompetitionList = new DashboardCompetitionList('.dashboard-custom');

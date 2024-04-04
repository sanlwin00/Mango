var dataTable;

$(document).ready(function () {
    loadDataTable();
});
function getQueryParamByName(name) {
    const searchParams = new URLSearchParams(window.location.search);
    return searchParams.get(name);
}

function loadDataTable() {
    var status = getQueryParamByName('status');
    dataTable = $('#dtOrder').DataTable({
        ajax: {
            url: "/Order/GetAllOrders", data: { status: status } },
        columns: [
            { data: 'orderHeaderId', width: '5%' },
            { data: 'orderDate', "width": '20%' },
            { data: 'email' },
            { data: 'firstName', "width": '10%' },
            { data: 'phone', "width": '10%' },
            { data: 'status', "width": '10%' },
            { data: 'orderTotal', "width": '10%' },
            {
                data: 'orderHeaderId',
                width: '5%',
                render: function (data) {
                    return `<a class="btn btn-outline-success" href="/Order/OrderDetail?orderId=${data}">
								<i class="bi bi-pencil-square"></i>
							</a>`;   
                }
            }
        ],
        columnDefs: [
            {
                targets: 1,
                render: DataTable.render.datetime()
            },
            {
                targets: 6,
                render: DataTable.render.number(null, null, 2, '$')
            }
        ],
        layout: {
            topStart: null,
            top: null,
            bottom: 'paging',
            bottomStart: 'info',
            bottomEnd: 'pageLength'
        },
        order: [[0, 'desc']]
    });
}
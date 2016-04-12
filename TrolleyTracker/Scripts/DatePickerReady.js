if (!Modernizr.inputtypes.date) {
    $(function () {

       $('.datetimepicker').datetimepicker({
		 defaultDate : moment().add(1, 'days'),
		format: 'MM/DD/YYYY'
});

    });
}

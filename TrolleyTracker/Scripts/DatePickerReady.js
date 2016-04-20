
    $(function () {

       $('.datetimepicker').datetimepicker({
		 defaultDate : moment().add(1, 'days'),
		format: 'MM/DD/YYYY'
        });

       $('.timeonlypicker').datetimepicker({
           format: 'LT'
       });
    });

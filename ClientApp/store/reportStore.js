import { fetch, addTask } from 'domain-task';
import moment from 'moment';

const initState = {
    all: true,
    id: null,
    options: '0',
    dateOptions: '0',
    fromDate: new moment(),
    toDate: new moment(),
    status: 0 // 0 = form, 1 = isLoading, 2 = table, -1 = failed
}

export const getDateOptions = (options) => {
    var today = new moment();
    switch (options) {
        case '0':
            return today.format('YYYY-MM-DD');
        case '1':
            return today.add(-7, 'days').format('YYYY-MM-DD');
        case '2':
            return today.add(-14, 'days').format('YYYY-MM-DD');
        case '3':
            return today.add(-30, 'days').format('YYYY-MM-DD');
        case '4':
            return today.add(-90, 'days').format('YYYY-MM-DD');
        case '-1':
            return;
        default:
            throw new Error('Options is invalid.');
    }
}
export const actionCreators = {
    onNameChanged: (value) => (dispatch, getState) => {
        dispatch({ type: 'ON_NAME_CHANGE', payload: { id: value } });
    },
    onOptionsChanges: (value) => (dispatch, getState) => {
        dispatch({ type: 'ON_OPT_CHANGE', payload: { options: value } });
    },
    onAllChanged: () => (dispatch, getState) => {
        var opt = !getState().report.all;
        dispatch({ type: 'ON_ALL_CHANGE', payload: { all: opt } })
    },
    onDateOptionsChanges: (value) => (dispatch, getState) => {
        dispatch({ type: 'ON_DATE_OPT_CHANGE', payload: { dateOptions: value } })
    },
    onFromDateChange: (value) => (dispatch, getState) => {
        var to = getState().report.toDate;
        if (value.isAfter(to))
            dispatch({ type: 'ON_FROM_DATE_CHANGE', payload: { fromDate: to, toDate: value } });
        else
            dispatch({ type: 'ON_FROM_DATE_CHANGE', payload: { fromDate: value } });
    },
    onToDateChange: (value) => (dispatch, getState) => {
        var from = getState().report.fromDate;
        if (value.isAfter(from))
            dispatch({ type: 'ON_FROM_DATE_CHANGE', payload: { toDate: value } });
        else
            dispatch({ type: 'ON_FROM_DATE_CHANGE', payload: { fromDate: value, toDate: from } });
    },
    query: () => (dispatch, getState) => {
        var props = getState().report;
        dispatch({ type: 'ON_REPORT_QUERY', payload: { status: 1 } });
        let fetchTask = fetch(`api/record/query`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                id: props.all ? '0' : props.id,
                options: props.options,
                fromDate: props.dateOptions === '-1' ?
                    props.fromDate.format('YYYY-MM-DD') : getDateOptions(props.dateOptions),
                toDate: props.dateOptions === '-1' ?
                    props.toDate.format('YYYY-MM-DD') : new moment().format('YYYY-MM-DD'),
            })
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'ON_REPORT_QUERY_FINISHED', payload: { status: 2 } });
        }).catch(error => {
            dispatch({ type: 'ON_REPORT_QUERY_FAILED', payload: { status: -1 } });
        });
        addTask(fetchTask);
    },
    onGoBackClick: () => (dispatch, getState) => {
        dispatch({ type: 'ON_GOBACK_CLICK' });
    }
}

export const reducer = (state = initState, action) => {
    var _data = Object.assign({}, state, action.payload);
    switch (action.type) {
        case 'ON_NAME_CHANGE':
        case 'ON_ALL_CHANGE':
        case 'ON_OPT_CHANGE':
        case 'ON_DATE_OPT_CHANGE':
        case 'ON_FROM_DATE_CHANGE':
        case 'ON_TO_DATE_CHANGE':
        case 'ON_REPORT_QUERY':
        case 'ON_REPORT_QUERY_FINISHED':
            return _data;
        case 'ON_REPORT_QUERY_FAILED':
            return _data;
        case 'ON_GOBACK_CLICK':
        default:
            return initState;
    }
}
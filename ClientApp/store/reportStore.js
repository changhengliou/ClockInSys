import { fetch, addTask } from 'domain-task';
import moment from 'moment';

const initState = {
    a: 0,
    s: 0,
    f: 0,
    t: -1,
    all: true,
    id: null,
    options: '0',
    dateOptions: '0',
    fromDate: new moment(),
    toDate: new moment(),
    status: 0, // 0 = form, 1 = isLoading, 2 = table, -1 = failed
    isLoading: false,
    data: [],
    showDialog: false,
    name: null,
    date: new moment(),
    model: {
        userName: '',
        checkedDate: null,
        checkInTime: '',
        checkOutTime: '',
        geoLocation1: '',
        geoLocation2: '',
        offType: '',
        offTimeStart: '',
        offTimeEnd: '',
        offReason: '',
        statusOfApproval: '審核中',
        recordId: null,
        userId: null
    }
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
        dispatch({ type: 'ON_REPORT_QUERY', payload: { status: 1, isLoading: true } });
        let fetchTask = fetch(`api/record/query`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                id: props.all ? null : props.id,
                options: props.options,
                fromDate: props.dateOptions === '-1' ?
                    props.fromDate.format('YYYY-MM-DD') : getDateOptions(props.dateOptions),
                toDate: props.dateOptions === '-1' ?
                    props.toDate.format('YYYY-MM-DD') : new moment().format('YYYY-MM-DD'),
            })
        }).then(response => response.json()).then(data => {
            dispatch({
                type: 'ON_REPORT_QUERY_FINISHED',
                payload: {
                    status: 2,
                    data: data.payload.model,
                    isLoading: false,
                    a: data.payload.a,
                    s: data.payload.s,
                    f: data.payload.f
                }
            });
        }).catch(error => {
            dispatch({ type: 'ON_REPORT_QUERY_FAILED', payload: { status: -1 } });
        });
        addTask(fetchTask);
    },
    onGoBackClick: () => (dispatch, getState) => {
        dispatch({ type: 'ON_GOBACK_BTN_CLICK' });
    },
    onChangingDataBtnClick: (e) => (dispatch, getState) => {
        var model = {...getState().report.data[e] };
        dispatch({ type: 'ON_CHANGING_DATA_BTN_CLICK', payload: { showDialog: true, model: model } });
    },
    onDialogClose: () => (dispatch, getState) => {
        dispatch({ type: 'ON_CHANGING_DATA_DIALOG_CLOSE', payload: { showDialog: false } });
    },
    onInputChange: (val, name) => (dispatch, getState) => {
        var payload = {};
        payload[name] = val;
        if (name === 'offType' && val === '') {
            payload.offTimeStart = '';
            payload.offTimeEnd = '';
            payload.offReason = '';
            payload.statusOfApproval = '審核中';
        }
        dispatch({ type: 'ON_INPUT_DATA_CHANGE', payload: payload });
    },
    onSubmitReport: () => (dispatch, getState) => {
        dispatch({ type: 'ON_REPORT_QUERY_EDIT', payload: { showDialog: false, isLoading: true } });
        var s = getState().report;
        var data = s.model;
        let fetchTask = fetch(`api/record/editRecord`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                userId: data.userId,
                recordId: data.recordId,
                checkedDate: typeof data.checkedDate === 'object' ?
                    data.checkedDate.format('YYYY-MM-DD') : data.checkedDate,
                checkInTime: data.checkInTime,
                checkOutTime: data.checkOutTime,
                geoLocation1: data.geoLocation1,
                geoLocation2: data.geoLocation2,
                offReason: data.offReason,
                offTimeStart: data.offTimeStart,
                offTimeEnd: data.offTimeEnd,
                offType: data.offType,
                statusOfApproval: data.statusOfApproval,
                id: s.all ? null : s.id,
                options: s.options,
                fromDate: s.dateOptions === '-1' ?
                    s.fromDate.format('YYYY-MM-DD') : getDateOptions(s.dateOptions),
                toDate: s.dateOptions === '-1' ?
                    s.toDate.format('YYYY-MM-DD') : new moment().format('YYYY-MM-DD'),
            })
        }).then(response => response.json()).then(data => {
            console.log(data.payload);
            dispatch({ type: 'ON_REPORT_QUERY_EDIT_FINISHED', payload: { data: data.payload, isLoading: false } });
        }).catch(error => {
            dispatch({ type: 'ON_REPORT_QUERY_EDIT_FAILED' });
        });
        addTask(fetchTask);
    },
    onDeleteReport: () => (dispatch, getState) => {
        dispatch({ type: 'ON_REPORT_QUERY_DROP', payload: { showDialog: false, isLoading: true } });
        var s = getState().report;
        var data = s.model;
        let fetchTask = fetch(`api/record/deleteRecord`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                RecordId: data.recordId,
                id: s.all ? null : s.id,
                options: s.options,
                fromDate: s.dateOptions === '-1' ?
                    s.fromDate.format('YYYY-MM-DD') : getDateOptions(s.dateOptions),
                toDate: s.dateOptions === '-1' ?
                    s.toDate.format('YYYY-MM-DD') : new moment().format('YYYY-MM-DD')
            })
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'ON_REPORT_QUERY_DROP_FINISHED', payload: { data: data.payload, isLoading: false } });
        }).catch(error => {
            dispatch({ type: 'ON_REPORT_QUERY_DROP_FAILED' });
        });
        addTask(fetchTask);
    },
    handleNameChange: (val) => (dispatch, getState) => {
        dispatch({ type: 'ON_NAME_QUERY_CHANGE', payload: { name: val } });
    },
    onDateChange: (val) => (dispatch, getState) => {
        dispatch({ type: 'ON_DATE_QUERY_CHANGE', payload: { date: val } });
    },
    onNewRecord: () => (dispatch, getState) => {
        var props = getState().report;
        let fetchTask = fetch(`api/record/query`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                id: props.name,
                options: props.options,
                fromDate: props.date.format('YYYY-MM-DD'),
                toDate: props.date.format('YYYY-MM-DD'),
            })
        }).then(response => response.json()).then(data => {
            var model;
            if (data.payload.model.length)
                model = data.payload.model[0];
            else {
                model = initState.model;
                model.checkedDate = getState().report.date;
                model.userId = getState().report.name;
            }
            dispatch({
                type: 'ON_NEW_RECORD_QUERY_FINISHED',
                payload: {
                    showDialog: true,
                    model: model,
                    a: data.payload.a,
                    s: data.payload.s,
                    f: data.payload.f
                }
            });
        }).catch(error => {
            dispatch({ type: 'ON_NEW_RECORD_QUERY_FAILED' });
        });
        addTask(fetchTask);
    },

}

export const reducer = (state = initState, action) => {
    switch (action.type) {
        case 'ON_NAME_CHANGE':
        case 'ON_ALL_CHANGE':
        case 'ON_OPT_CHANGE':
        case 'ON_DATE_OPT_CHANGE':
        case 'ON_FROM_DATE_CHANGE':
        case 'ON_TO_DATE_CHANGE':
        case 'ON_REPORT_QUERY':
        case 'ON_REPORT_QUERY_FINISHED':
        case 'ON_CHANGING_DATA_BTN_CLICK':
        case 'ON_CHANGING_DATA_DIALOG_CLOSE':
        case 'ON_REPORT_QUERY_DROP':
        case 'ON_REPORT_QUERY_DROP_FINISHED':
        case 'ON_REPORT_QUERY_EDIT':
        case 'ON_REPORT_QUERY_EDIT_FINISHED':
        case 'ON_NAME_QUERY_CHANGE':
        case 'ON_DATE_QUERY_CHANGE':
        case 'ON_NEW_RECORD_QUERY_FINISHED':
            return Object.assign({}, state, action.payload);
        case 'ON_INPUT_DATA_CHANGE':
            var s = {...state };
            s.model = {...state.model, ...action.payload };
            return s;
        case 'ON_REPORT_QUERY_FAILED':
        case 'ON_REPORT_QUERY_DROP_FAILED':
        case 'ON_REPORT_QUERY_EDIT_FAILED':
        case 'ON_NEW_RECORD_QUERY_FAILED':
            return Object.assign({}, state, action.payload);
        case 'ON_GOBACK_BTN_CLICK':
        default:
            return initState;
    }
}
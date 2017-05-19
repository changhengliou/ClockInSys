import { fetch, addTask } from 'domain-task';
import moment from 'moment';

const initState = {
    isLoading: false,
    showDialog: false,
    disabledContent: 'none',
    fromDate: null,
    toDate: null,
    fromTime: '13',
    toTime: '18',
    offType: '事假',
    offReason: '',
    events: []
}
export const actionCreators = {
    getInitState: (year, month) => (dispatch, getState) => {
        dispatch({ type: 'REQUEST_RECORD_INIT_STATE', payload: { isLoading: true } });
        let fetchTask = fetch(`api/record/getInitState?y=${year}&m=${month}`, {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            }
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'REQUEST_RECORD_INIT_STATE_FINISHED', payload: { isLoading: false, events: data.payload } });
        }).catch(error => {
            dispatch({ type: 'REQUEST_RECORD_INIT_STATE_FAILED' });
        });
        addTask(fetchTask);
    },
    onTimeChange: (name, value) => (dispatch, getState) => {
        var fromTime = parseFloat(getState().dayoff.fromTime);
        var toTime = parseFloat(getState().dayoff.toTime);
        var _value = parseFloat(value);
        if (name === 'fromTime') {
            if (_value > toTime) {
                dispatch({ type: 'ON_OFFTIME_CHANGED', payload: { fromTime: toTime, toTime: _value } });
            } else {
                dispatch({ type: 'ON_OFFTIME_CHANGED', payload: { fromTime: _value } });
            }
        } else if (name === 'toTime') {
            if (fromTime > _value) {
                dispatch({ type: 'ON_OFFTIME_CHANGED', payload: { fromTime: _value, toTime: fromTime } });
            } else {
                dispatch({ type: 'ON_OFFTIME_CHANGED', payload: { toTime: _value } });
            }
        }
    },
    onDateChange: (date) => (dispatch, getState) => {
        var state = getState().dayoff;
        if (state.fromDate !== date)
            dispatch({ type: 'ON_OFFDATE_CHANGED', payload: { toDate: date, fromTime: '9', toTime: '18' } });
        else
            dispatch({ type: 'ON_OFFDATE_CHANGED', payload: { toDate: date } });
    },
    onOffTypeChange: (value) => (dispatch, getState) => {
        dispatch({ type: 'ON_OFFTYPE_CHANGED', payload: { offType: value } });
    },
    onOffReasonChange: (value) => (dispatch, getState) => {
        dispatch({ type: 'ON_OFFREASON_CHANGED', payload: { offReason: value } });
    },
    onDialogOpen: (e, option) => (dispatch, getState) => {
        if (!option) {
            dispatch({
                type: 'ON_DIALOG_OPEN',
                payload: {
                    showDialog: true,
                    fromDate: new moment(e).format('YYYY-MM-DD'),
                    toDate: new moment(e).format('YYYY-MM-DD'),
                    offType: '事假',
                    offReason: ''
                }
            });
        } else {
            var payload = {
                showDialog: true,
                disabledContent: 'all',
                fromDate: new moment(e.checkedDate).format('YYYY-MM-DD'),
                toDate: new moment(e.offEndDate).format('YYYY-MM-DD'),
                fromTime: parseInt(e.offTimeStart.slice(0, 2)).toString(),
                toTime: parseInt(e.offTimeEnd.slice(0, 2)).toString(),
                offType: e.offType,
                offReason: e.offReason
            }
            if (option === 'fixable') {
                payload.disabledContent = 'partial';
            }
            dispatch({ type: 'ON_DIALOG_OPEN', payload: payload });
        }
    },
    onDialogClose: () => (dispatch, getState) => {
        dispatch({ type: 'ON_DIALOG_CLOSE', payload: { showDialog: false, disabledContent: 'none' } });
    },
    onDialogConfirm: () => (dispatch, getState) => {
        dispatch({ type: 'PROCEED_APPLY_OFF', payload: { showDialog: false, isLoading: true } });
        var state = getState().dayoff;
        let fetchTask = fetch('api/record/applyDayOff', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                CheckedDate: state.fromDate,
                OffEndDate: state.toDate,
                FromTime: state.fromTime,
                ToTime: state.toTime,
                OffType: state.offType,
                OffReason: state.offReason,
            })
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'PROCEED_APPLY_OFF_FINISHED', payload: { events: data.payload, isLoading: false } });
        }).catch(error => {
            dispatch({ type: 'PROCEED_APPLY_OFF_FAILED', payload: error });
        });
        addTask(fetchTask);
    },
    onCancelLeaves: (date) => (dispatch, getState) => {
        dispatch({ type: 'PROCEED_APPLY_CANCEL', payload: { showDialog: false, isLoading: true } });
        let fetchTask = fetch(`api/record/cancelDayOff?d=${date}`, {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            }
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'PROCEED_APPLY_CANCEL_FINISHED', payload: { events: data.payload, isLoading: false } });
        }).catch(error => {
            dispatch({ type: 'PROCEED_APPLY_CANCEL_FAILED', payload: error });
        });
        addTask(fetchTask);
    }
};



export const reducer = (state = initState, action) => {
    var _data = Object.assign({}, state, action.payload);
    switch (action.type) {
        case 'REQUEST_RECORD_INIT_STATE':
        case 'REQUEST_RECORD_INIT_STATE_FINISHED':
        case 'ON_OFFDATE_CHANGED':
        case 'ON_OFFTIME_CHANGED':
        case 'ON_OFFTYPE_CHANGED':
        case 'ON_OFFREASON_CHANGED':
        case 'ON_DIALOG_OPEN':
        case 'ON_DIALOG_CLOSE':
        case 'PROCEED_APPLY_OFF':
        case 'PROCEED_APPLY_OFF_FINISHED':
        case 'PROCEED_APPLY_CANCEL':
        case 'PROCEED_APPLY_CANCEL_FINISHED':
            return _data;
        case 'PROCEED_APPLY_OFF_FAILED':
        case 'PROCEED_APPLY_CANCEL_FAILED':
        case 'REQUEST_RECORD_INIT_STATE_FAILED':
            return _data;
        default:
            return state;
    }
};
import { fetch, addTask } from 'domain-task';
import moment from 'moment';

const initState = {
    isLoading: false,
    showDialog: false,
    data: [],
    model: {
        userName: '',
        checkedDate: '',
        offTimeStart: '',
        offTimeEnd: '',
        offType: '',
        offReason: ''
    }
}

export const actionCreators = {
    getInitState: (year, month) => (dispatch, getState) => {
        dispatch({ type: 'REQUEST_ABSENCE_INIT_STATE', payload: { isLoading: true } });
        let fetchTask = fetch(`api/record/GetAbsentStatus?y=${year}&m=${month}`, {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            }
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'REQUEST_ABSENCE_INIT_STATE_FINISHED', payload: { isLoading: false, data: data.payload } });
        }).catch(error => {
            dispatch({ type: 'REQUEST_ABSENCE_INIT_STATE_FAILED' });
        });
        addTask(fetchTask);
    },
    onDialogOpen: (info) => (dispatch, getState) => {
        var model = {
            userName: info.userName,
            checkedDate: info.checkedDate,
            offTimeStart: info.offTimeStart,
            offTimeEnd: info.offTimeEnd,
            offType: info.offType,
            offReason: info.offReason
        }
        dispatch({ type: 'ON_ABSENCE_DIALOG_OPEN', payload: { showDialog: true, model: model } });
    },
    onDialogClose: () => (dispatch, getState) => {
        dispatch({ type: 'ON_ABSENCE_DIALOG_CLOSE', payload: { showDialog: false } });
    }
}

export const reducer = (state = initState, action) => {
    var _data = Object.assign({}, state, action.payload);
    switch (action.type) {
        case 'REQUEST_ABSENCE_INIT_STATE':
        case 'REQUEST_ABSENCE_INIT_STATE_FINISHED':
        case 'ON_ABSENCE_DIALOG_OPEN':
        case 'ON_ABSENCE_DIALOG_CLOSE':
            return _data;
        case 'REQUEST_ABSENCE_INIT_STATE_FAILED':
            return _data;
        default:
            return initState;
    }
}
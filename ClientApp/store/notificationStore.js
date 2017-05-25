import { fetch, addTask } from 'domain-task';
import moment from 'moment';

const initState = {
    showDialog: false,
    t: -1,
    isLoading: false,
    selfLoading: false,
    data: [],
    selfData: []
}

export const actionCreators = {
    getInitState: () => (dispatch, getState) => {
        dispatch({ type: 'REQUEST_NOTIFY_INIT_STATE', payload: { isLoading: true } });
        let fetchTask = fetch(`api/record/getInitNotifiedState`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            }
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'REQUEST_NOTIFY_INIT_STATE_FINISHED', payload: { isLoading: false, data: data.payload } });
        }).catch(error => {
            dispatch({ type: 'REQUEST_NOTIFY_INIT_STATE_FAILED' });
        });
        addTask(fetchTask);
    },
    getSelfState: () => (dispatch, getState) => {
        dispatch({ type: 'REQUEST_NOTIFY_SELF_STATE', payload: { selfLoading: true } });
        let fetchTask = fetch(`api/record/getSelfNotifiedState`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            }
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'REQUEST_NOTIFY_SELF_STATE_FINISHED', payload: { selfLoading: false, selfData: data.payload } });
        }).catch(error => {
            dispatch({ type: 'REQUEST_NOTIFY_SELF_STATE_FAILED' });
        });
        addTask(fetchTask);
    },
    handleClick: (index, val) => (dispatch, getState) => {
        let fetchTask = fetch(`api/record/setNotification`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                recordId: getState().notification.data[index].id,
                status: val === 'approve' ? '已核准' : '遭駁回'
            })
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'PROCEED_NOTIFY_STATUS_FINISHED', payload: { data: data.payload } });
        }).catch(error => {
            dispatch({ type: 'REQUEST_NOTIFY_STATUS_FAILED' });
        });
        addTask(fetchTask);
    },
    showDialog: (index) => (dispatch, getState) => {
        var date = new moment(getState().notification.selfData[index].checkedDate, 'YYYY-MM-DD');
        var now = new moment().set({ hour: 9, minute: 0 });
        console.log(date.format('YYYY-MM-DD'), now.format('YYYY-MM-DD'));
        if (now.isAfter(date))
            dispatch({ type: 'SHOW_NOTIFY_DIALOG', payload: { showDialog: true, t: -1 } });
        else
            dispatch({ type: 'SHOW_NOTIFY_DIALOG', payload: { showDialog: true, t: index } });
    },
    closeDialog: () => (dispatch, getState) => {
        dispatch({ type: 'CLOSE_NOTIFY_DIALOG', payload: { showDialog: false, t: -1 } });
    },
    onRemoveRecord: (index) => (dispatch, getState) => {
        var date = getState().notification.selfData[index].checkedDate;
        dispatch({ type: 'PROCEED_RECORD_REMOVE', payload: { showDialog: false, t: -1 } });
        let fetchTask = fetch(`api/record/cancelDayOff?d=${date}`, {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            }
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'PROCEED_RECORD_REMOVE_FINISHED', payload: { selfData: data.payload, isLoading: false } });
        }).catch(error => {
            dispatch({ type: 'PROCEED_RECORD_REMOVE_FAILED', payload: error });
        });
        addTask(fetchTask);
    }
}

export const reducer = (state = initState, action) => {
    switch (action.type) {
        case 'REQUEST_NOTIFY_INIT_STATE':
        case 'REQUEST_NOTIFY_INIT_STATE_FINISHED':
        case 'REQUEST_NOTIFY_SELF_STATE':
        case 'REQUEST_NOTIFY_SELF_STATE_FINISHED':
        case 'SHOW_NOTIFY_DIALOG':
        case 'CLOSE_NOTIFY_DIALOG':
        case 'PROCEED_RECORD_REMOVE':
        case 'PROCEED_RECORD_REMOVE_FINISHED':
        case 'PROCEED_NOTIFY_STATUS_FINISHED':
            return {...state, ...action.payload };
        case 'REQUEST_NOTIFY_INIT_STATE_FAILED':
        case 'REQUEST_NOTIFY_SELF_STATE_FAILED':
        case 'PROCEED_RECORD_REMOVE_FAILED':
        default:
            return initState;
    }
}
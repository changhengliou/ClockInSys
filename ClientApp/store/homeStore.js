import { Reducer } from 'redux';
import { fetch, addTask } from 'domain-task';
import moment from 'moment';

const initState = {
    showDialog: false,
    showOTDialog: false,
    status: null,
    OTDate: new moment(),
    OTTime: '19:00',
    data: {
        shouldCheckInDisable: true,
        shouldCheckOutDisable: true,
        currentDate: null,
        currentTime: null,
        checkIn: null,
        checkOut: null,
        offStatus: null
    }
};

export const actionCreators = {
    getInitState: () => (dispatch, getState) => {
        let fetchTask = fetch('api/home/getInitState', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            }
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'REQUEST_INIT_STATE', payload: { data: data.payload } });
        }).catch(error => {
            dispatch({ type: 'REQUEST_INIT_STATE_FAILED', payload: error });
        });
        addTask(fetchTask);
    },
    timeTicking: (time) => (dispatch, getState) => {
        time = time.add(1, 'seconds').format("HH:mm:ss A");
        dispatch({ type: 'TIME_TICKING', payload: { currentTime: time } });
    },
    proceedCheck: (_geo, type) => (dispatch, getState) => {
        var URL = 'api/home/proceedCheckIn';
        if (type === 'checkOut') {
            URL = 'api/home/proceedCheckOut';
        }
        let fetchTask = fetch(URL, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                'Geo': _geo
            })
        }).then(response => response.json()).then(data => {
            if (!data.status)
                throw new Error('Something goes wrong =(');
            else
                dispatch({ type: 'PROCEED_CHECKING', payload: { data: data.payload, showDialog: true } });
        }).catch(error => {
            dispatch({ type: 'REQUEST_CHECKING_FAILED', payload: error });
        });
        addTask(fetchTask);
    },
    onCloseDialog: () => (dispatch, getState) => {
        dispatch({ type: 'ON_HOME_PAGE_DIALOG_CLOSE', payload: { showDialog: false } });
    },
    onOpenOTDialog: () => (dispatch, getState) => {
        dispatch({ type: 'ON_HOME_PAGE_OT_DIALOG_OPEN', payload: { showOTDialog: true } });
    },
    onCloseOTDialog: () => (dispatch, getState) => {
        dispatch({ type: 'ON_HOME_PAGE_OT_DIALOG_CLOSE', payload: { showOTDialog: false } })
    },
    onOTDateChange: (state) => (dispatch, getState) => {
        dispatch({ type: 'ON_HOME_PAGE_OT_DATE_CHANGE', payload: { OTDate: state } })
    },
    onOTTimeChange: (state) => (dispatch, getState) => {
        dispatch({ type: 'ON_HOME_PAGE_OT_TIME_CHANGE', payload: { OTTime: state } })
    },
    onOTSubmit: (date, time) => (dispatch, getState) => {
        date = new moment(date, 'YYYY/MM/DD').format('YYYY/MM/DD')
        let fetchTask = fetch(`api/home/OTsubmit?d=${date}&t=${time}`, {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
        }).then(response => response.json()).then(data => {
            console.log(data)
            dispatch({ type: 'ON_HOME_PAGE_OT_SUBMIT', payload: { showDialog: true, showOTDialog: false } });
        }).catch(error => {
            dispatch({ type: 'ON_HOME_PAGE_OT_SUBMIT_FAILED', payload: error });
        });
        addTask(fetchTask);
    },
};



export const reducer = (state = initState, action) => {
    switch (action.type) {
        case 'REQUEST_INIT_STATE':
            return {...state, ...action.payload };
        case 'REQUEST_INIT_STATE_FAILED':
            return { status: 'ERROR', error: action.payload };
        case 'PROCEED_CHECKING':
            var result = {...state, ...action.payload };
            result.data = {...state.data, ...action.payload.data }
            return result;
        case 'REQUEST_CHECKING_FAILED':
            return { status: 'ERROR', error: action.payload };
        case 'TIME_TICKING':
            var _data = Object.assign({}, state.data, action.payload);
            return Object.assign({}, state, { data: _data });
        case 'ON_HOME_PAGE_DIALOG_CLOSE':
        case 'ON_HOME_PAGE_OT_DIALOG_OPEN':
        case 'ON_HOME_PAGE_OT_DIALOG_CLOSE':
        case 'ON_HOME_PAGE_OT_DATE_CHANGE':
        case 'ON_HOME_PAGE_OT_TIME_CHANGE':
        case 'ON_HOME_PAGE_OT_SUBMIT':
            return {...state, ...action.payload };
        default:
            return state;
    }
};
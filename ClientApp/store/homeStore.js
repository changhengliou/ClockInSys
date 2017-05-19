import { Reducer } from 'redux';
import { fetch, addTask } from 'domain-task';
import moment from 'moment';

const initState = {
    status: null,
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
            dispatch({ type: 'REQUEST_INIT_STATE', payload: data.payload });
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
            console.log(data);
            if (!data.status)
                throw new error('Something goes wrong =(');
            else
                dispatch({ type: 'PROCEED_CHECKING', payload: data.payload });
        }).catch(error => {
            dispatch({ type: 'REQUEST_CHECKING_FAILED', payload: error });
        });
        addTask(fetchTask);
    }
};



export const reducer = (state = initState, action) => {
    var _data = Object.assign({}, state.data, action.payload);
    switch (action.type) {
        case 'REQUEST_INIT_STATE':
            return { status: 'SUCCESS', data: _data };
        case 'REQUEST_INIT_STATE_FAILED':
            return { status: 'ERROR', error: action.payload };
        case 'PROCEED_CHECKING':
            return { status: 'SUCCESS', data: _data };
        case 'REQUEST_CHECKING_FAILED':
            return { status: 'ERROR', error: action.payload };
        case 'TIME_TICKING':
            return Object.assign({}, state, { data: _data });
        default:
            return state;
    }
};
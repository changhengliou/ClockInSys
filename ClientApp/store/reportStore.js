import { fetch, addTask } from 'domain-task';
import moment from 'moment';

const initState = {}

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
}

export const reducer = (state = initState, action) => {
    var _data = Object.assign({}, state, action.payload);
    switch (action.type) {
        default: return initState;
    }
}
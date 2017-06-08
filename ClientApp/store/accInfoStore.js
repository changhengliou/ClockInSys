import { Action, Reducer } from 'redux';
import { fetch, addTask } from 'domain-task';

export const initState = {
    roles: [],
    loadingCompleted: false,
    userName: null,
    userEmail: null,
    userPhone: null,
    dateOfEmployment: null,
    jobTitle: null,
    annualLeaves: null,
    sickLeaves: null,
    familyCareLeaves: null,
    deputy: null,
    supervisor: null
};

export const actionCreators = {
    getInitState: () => (dispatch, getState) => {
        let fetchTask = fetch('api/account/getInitState', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            }
        }).then(response => response.json()).then(data => {
            dispatch({ type: 'REQUEST_ACCOUNTINFO', payload: data.payload });
        }).catch(error => {
            dispatch({ type: 'REQUEST_ACCOUNTINFO_FAILED', payload: error });
        });
        addTask(fetchTask);
    }
};



export const reducer = (state = initState, action) => {
    switch (action.type) {
        case 'REQUEST_ACCOUNTINFO':
            var _data = Object.assign({}, state, action.payload, { loadingCompleted: true });
            return _data;
        case 'REQUEST_ACCOUNTINFO_FAILED':
            return initState;
        default:
            return state;
    }
};
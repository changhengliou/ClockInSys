import { fetch, addTask } from 'domain-task';
import moment from 'moment';

const initState = {
    all: false,
    name: null,
    options: '0',
    dateOptions: null,
    nameList: [],
}

export const actionCreators = {
    onNameChanged: (value) => (dispatch, getState) => {
        dispatch({ type: 'ON_NAME_CHANGE', payload: { name: value } });
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
}

export const reducer = (state = initState, action) => {
    var _data = Object.assign({}, state, action.payload);
    switch (action.type) {
        case 'ON_NAME_CHANGE':
        case 'ON_ALL_CHANGE':
        case 'ON_OPT_CHANGE':
        case 'ON_DATE_OPT_CHANGE':
            return _data;
        default:
            return initState;
    }
}
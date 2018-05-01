import { fetch, addTask } from 'domain-task';
import { browserHistory } from 'react-router';

const initState = {
    showTable: false,
    isLoading: true,
    data: [],
    count: 0,
    errorList: null,
    errorCount: 0
}

const errorCount = (data, callback) => {
    var x = [];
    for (var i = 0; i <= data.length; i++) {
        if (i === data.length) {
            callback({
                type: 'UPLOAD_ERROR_REPORT',
                payload: {
                    errorList: x.join(),
                    count: data.length,
                    errorCount: x.length
                }
            });
            break;
        }
        if (!data[i].isDataValid) {
            x.push(i + 1);
            continue;
        }
        if (data[i].checkInTime && !data[i].isInValid) {
            x.push(i + 1);
            continue;
        }
        if (data[i].checkOutTime && !data[i].isOutValid) {
            x.push(i + 1);
            continue;
        }
        if (data[i].overtimeEndTime && !data[i].isOTValid) {
            x.push(i + 1);
            continue;
        }
        if ((data[i].offApplyDate || data[i].offReason ||
                data[i].offTime || data[i].offType) &&
            !data[i].isOffValid) {
            x.push(i + 1);
            continue;
        }
    }
}

const uploadData = (data, callback) => {
    var x = [];
    for (var i = 0; i <= data.length; i++) {
        if (i === data.length) {
            callback(x);
            break;
        }
        if (!data[i].isDataValid) {
            continue;
        }
        if (data[i].checkInTime && !data[i].isInValid) {
            continue;
        }
        if (data[i].checkOutTime && !data[i].isOutValid) {
            continue;
        }
        if (data[i].overtimeEndTime && !data[i].isOTValid) {
            continue;
        }
        if ((data[i].offApplyDate || data[i].offReason ||
                data[i].offTime || data[i].offType) &&
            !data[i].isOffValid) {
            continue;
        }
        x.push(data[i]);
    }
}
export const actionCreators = {
    uploadFile: (val) => (dispatch, getState) => {
        dispatch({
            type: 'UPLOAD_FILE_BEGIN',
            payload: {
                showTable: true,
                isLoading: true
            }
        });
        var data = new FormData();
        data.append('file', val[0]);
        let fetchTask = fetch('/home/upload', {
            method: 'POST',
            credentials: 'include',
            body: data
        }).then(response => response.json()).then(data => {
            dispatch({
                type: 'UPLOAD_FILE_SUCCESS',
                payload: {
                    data: data.payload,
                    showTable: true,
                    isLoading: false
                }
            });
            errorCount(data.payload, dispatch);
        }).catch(error => {
            dispatch({ type: 'UPLOAD_FILE_FAILED', payload: error });
        });
        addTask(fetchTask);
    },
    goBackClick: () => (dispatch, getState) => {
        dispatch({ type: 'INITIALIZE_STATE' });
    },
    submitFile: () => (dispatch, getState) => {
        var data = getState().bulkyupdate.data;
        // var form = new FormData();
        uploadData(data, (_data) => {
            // form.append('list', _data);
            let fetchTask = fetch('/home/bulkyUpdateDatabase', {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Accept': 'application/json, text/javascript, */*; q=0.01',
                    'Content-Type': 'application/json; charset=UTF-8',
                },
                body: JSON.stringify({
                    list: _data
                })
            }).then(response => response.json()).then(data => {
                dispatch({ type: 'SUBMIT_RESULT_SUCCESS' });
                browserHistory.push(`/bulkyUpdate/result/${data.newCount}?count=${data.count}`);
            }).catch(error => {
                dispatch({ type: 'SUBMIT_RESULT_FAILED', payload: error });
            });
            addTask(fetchTask);
        });
    }
}
export const reducer = (state = initState, action) => {
    switch (action.type) {
        case 'UPLOAD_FILE_BEGIN':
        case 'UPLOAD_FILE_SUCCESS':
        case 'UPLOAD_ERROR_REPORT':
            return {...state, ...action.payload }
        case 'UPLOAD_FILE_FAILED':
            return state;
        default:
            return initState;
    }
}
import * as ManageAccount from './manageAccStore';
import * as __info__ from './accInfoStore';
import * as Home from './homeStore';
import * as DayOff from './dayoffStore';
import * as Notification from './notificationStore';
import * as Absent from './absentStore';
import * as Report from './reportStore';

export const actionCreators = {
    onNavBarOpen: (value) => (dispatch, getState) => {
        dispatch({ type: 'ON_NAV_BAR_OPEN', payload: { __s__: '300px' } });
    },
    onNavBarClose: (value) => (dispatch, getState) => {
        if (getState().__w3x__.__s__ === '0px')
            return;
        dispatch({ type: 'ON_NAV_BAR_CLOSE', payload: { __s__: '0px' } });
    }
}
export const reducer = (state = { __s__: '0px' }, action) => {
    switch (action.type) {
        case 'ON_NAV_BAR_OPEN':
        case 'ON_NAV_BAR_CLOSE':
            return {...state, ...action.payload };
        default:
            return state;
    }
}
export const reducers = {
    __w3x__: reducer,
    __info__: __info__.reducer,
    home: Home.reducer,
    dayoff: DayOff.reducer,
    manageAccount: ManageAccount.reducer,
    notification: Notification.reducer,
    absent: Absent.reducer,
    report: Report.reducer
};
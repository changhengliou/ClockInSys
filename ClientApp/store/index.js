import * as ManageAccount from './manageAccStore';
import * as __info__ from './accInfoStore';
import * as Home from './homeStore';
import * as DayOff from './dayoffStore';
import * as Notification from './notificationStore';
import * as Absent from './absentStore';
import * as Report from './reportStore';

export const reducers = {
    __info__: __info__.reducer,
    home: Home.reducer,
    dayoff: DayOff.reducer,
    manageAccount: ManageAccount.reducer,
    notification: Notification.reducer,
    absent: Absent.reducer,
    report: Report.reducer
};
import * as React from 'react';
import { Router, Route, HistoryBase } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import ManageAccount from './components/ManageAccount';
import Confirm from './components/ManageAccount.Confirm';
import DayOff from './components/DayOff';
import DayOffCalendar from './components/DayOffCalendar';
import AccountInfo from './components/AccountInfo';
import Notification from './components/Notificaion';
import AbsenceStatus from './components/AbsenceStatus';
import Report from './components/Report';

export const UrlMapping = {
    '/': '打卡',
    '/dayOff': '請假申請',
    '/notification': '審核/通知',
    '/absent': '缺席狀態',
    '/manageaccount': '帳號管理',
    '/accountinfo': '帳戶資訊',
    '/report': '綜合報告'
};

export default (
    <Route component={ Layout }>
        <Route path='/' components={{ body: Home }}/>
        <Route path='/dayOff' components={{ body: DayOffCalendar }}/>
        <Route path='/notification' components={{ body: Notification }}/>
        <Route path='/absent' components={{ body: AbsenceStatus }}/>
        <Route path='/manageaccount' components={{ body: ManageAccount }} />
        <Route path='/manageaccount/confirm' component={{ body: Confirm }}>
            <Route path='(:msg)'/>
        </Route>
        <Route path='/accountinfo' components={{ body: AccountInfo }} />
        <Route path='/report' components={{ body: Report }} />
    </Route>
);

// Enable Hot Module Replacement (HMR)
if (module.hot) {
    module.hot.accept();
}

//{/*<Route path='/fetchdata' components={{ body: FetchData }}>*/}
//    {/*<Route path='(:startDateIndex)' /> { /* Optional route segment that does not affect NavMenu highlighting */ }*/}
//{/*</Route>}*/}
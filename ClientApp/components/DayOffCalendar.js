import React, { Component } from 'react';
import moment from 'moment';
import BigCalendar from 'react-big-calendar';
import { connect } from 'react-redux';
import 'react-big-calendar/lib/css/react-big-calendar.css';
import Dialog from 'rc-dialog';
import 'rc-dialog/assets/index.css';
import { actionCreators } from '../store/dayoffStore';
import DayOff from './DayOff';

BigCalendar.setLocalizer(BigCalendar.momentLocalizer(moment));

class DayOffCalendar extends Component {
    constructor(props) {
        super(props);
        this.onSelectSlot = this.onSelectSlot.bind(this);
    }
    
    componentWillMount() {
        var date = new Date();
        this.props.getInitState(date.getFullYear(), date.getMonth() + 1);
    }

    onSelectSlot(info) {
        var events = this.props.data.events;
        var date = new moment(info.start, 'YYYY-MM-DD').format('YYYY-MM-DD')
        for(var i = 0; i < events.length; i++) {
            if(events[i].checkedDate === date) {
                this.props.onDialogOpen(events[i], true);
                return;
            }
        } 
        this.props.onDialogOpen(info);
    }
    
    render() {
        var props = this.props.data;
        return (
            <div style={{height: '500px', width: '92%', margin: '0 auto'}}>
                <BigCalendar
                    selectable={ true }
                    onSelectSlot={ this.onSelectSlot }
                    onSelectEvent={ (info) => this.props.onDialogOpen(info, true) }
                    onNavigate={ (e) => { this.props.getInitState(e.getFullYear(), e.getMonth() + 1) } }
                    views={['month']}
                    culture={'zh-TW'}
                    titleAccessor='offType'
                    startAccessor='checkedDate'
                    endAccessor='offEndDate'
                    events={ props.events }/>
                <Dialog title={ props.disabledContent === 'none' ? '請假申請' : '請假紀錄' } 
                        className='rc-dialog-dayoff-header'
                        visible={ props.showDialog } 
                        onClose={ () => { this.props.onDialogClose() } }
                        animation="zoom"
                        maskAnimation="fade"
                        style={{ top: '6%' }}>
                    <DayOff/>
                </Dialog>
                <Dialog title='請稍後' 
                        className='rc-dialog-dayoff-header'
                        visible={ props.isLoading } 
                        style={{ top: '40%', textAlign: 'center'}}>
                    載入中...
                </Dialog>
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {
        data: state.dayoff
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        getInitState: (y, m) => {
            dispatch(actionCreators.getInitState(y, m));
        },
        onDialogOpen: (e, isContent = false) => {
            var time = e.start ? new moment(e.start) : new moment(e.checkedDate, 'YYYY-MM-DD');
            time.set({hour: 12, minute: 0});
            if(new moment().isAfter(time)){
                if (isContent) {
                    dispatch(actionCreators.onDialogOpen(e, 'disabled')); // show past record, disabled all
                    return;
                } else
                    return; // disable past date
            } 
            if(isContent)
                dispatch(actionCreators.onDialogOpen(e, 'fixable')); // show future record, fixable
            else
                dispatch(actionCreators.onDialogOpen(e.start)); // show future date  
        },
        onDialogClose: () => {
            dispatch(actionCreators.onDialogClose());
        }
    }
}
const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(DayOffCalendar);
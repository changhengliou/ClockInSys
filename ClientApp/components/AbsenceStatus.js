import React, { Component } from 'react';
import { connect } from 'react-redux';
import moment from 'moment';
import { actionCreators } from '../store/absentStore';
import ReactTable from 'react-table'
import BigCalendar from 'react-big-calendar';
import Dialog from 'rc-dialog';
import 'rc-dialog/assets/index.css';
import 'react-big-calendar/lib/css/react-big-calendar.css';
import 'react-table/react-table.css'
import { style } from './Report.Dialog';

BigCalendar.setLocalizer(BigCalendar.momentLocalizer(moment));

class AbsenceStatus extends Component {
    
    componentWillMount() {
        var now = new Date();
        this.props.getInitState(now.getFullYear(), now.getMonth() + 1);
    }
    
    render() {
        var props= this.props.data;
        return (
            <div style={{height: '500px', width: '92%', margin: '0 auto'}}>
                <div className='div_dot'>
                    <div className='dot red_dot'></div>
                    <span className='label_dot'>審核中</span>
                    <div className='dot blue_dot'></div>
                    <span className='label_dot'>已核准</span>
                </div>
                <BigCalendar
                    selectable={ true }
                    popup={ true }
                    eventPropGetter={ (obj, start, end, isSelected) => {
                        if (obj.statusOfApproval === '審核中')
                            return { style: { backgroundColor: 'red'} }
                        return { style: null };
                    } }
                    onSelectEvent={ (info) => this.props.onDialogOpen(info) }
                    onNavigate={ (e) => { this.props.getInitState(e.getFullYear(), e.getMonth() + 1) } }
                    views={['month']}
                    culture={'zh-TW'}
                    titleAccessor='userName'
                    startAccessor='checkedDate'
                    endAccessor='checkedDate'
                    events={ props.data }/>
                <Dialog title={ '請假紀錄' }
                        className='rc-dialog-dayoff-header'
                        visible={ props.showDialog } 
                        onClose={ () => { this.props.onDialogClose() } }
                        animation="zoom"
                        maskAnimation="fade"
                        style={{ top: '6%' }}>
                        <div style={ style.title }>{ props.model.userName }</div>
                        <div style={ style.title }>{ props.model.checkedDate }</div>
                        <div style={ style.wrapper }>
                            <span style={ style.label }>請假類別:</span>
                            <span style={ style.input }>{ props.model.offType }</span>
                        </div>
                        <div style={ style.wrapper }>
                            <span style={ style.label }>請假時間:</span>
                            <span style={ style.input }>
                                { `${props.model.offTimeStart} - ${props.model.offTimeEnd}` }
                            </span>
                        </div>
                        <div style={ style.wrapper }>
                            <span style={ style.label }>請假原因:</span>
                            <span style={ style.input }>{ props.model.offReason }</span>
                        </div>
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
        data: state.absent
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        getInitState: (y, m) => {
            dispatch(actionCreators.getInitState(y, m));
        },
        onDialogOpen: (info) => {
            dispatch(actionCreators.onDialogOpen(info));
        },
        onDialogClose: () => {
            dispatch(actionCreators.onDialogClose());
        }
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(AbsenceStatus);

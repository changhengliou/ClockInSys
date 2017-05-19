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

BigCalendar.setLocalizer(BigCalendar.momentLocalizer(moment));
const data = [
    {
        name: 'Tanner Linsley',
        age: 26,
        friend: {
            name: 'Jason Maurer',
            age: 23
        }
    }
];
const columns = [
    {
        Header: 'Name',
        accessor: 'name' 
    }, {
        Header: 'Age',
        accessor: 'age',
        Cell: props => <span className='number'>{props.value}</span>
    }, {
        id: 'friendName', // Required because our accessor is not a string
        Header: 'Friend Name',
        accessor: d => d.friend.name // Custom value accessors!
    }, {
        Header: props => <span>Friend Age</span>, // Custom header components!
        accessor: 'friend.age'
    }
];

class AbsenceStatus extends Component {
    render() {
        var props= this.props.data;
        return (
            <div style={{height: '500px', width: '92%', margin: '0 auto'}}>
                <BigCalendar
                    selectable={ true }
                    onSelectEvent={ (info) => this.props.onDialogOpen(info) }
                    onNavigate={ (e) => { this.props.getInitState(e.getFullYear(), e.getMonth() + 1) } }
                    views={['month']}
                    culture={'zh-TW'}
                    titleAccessor='offType'
                    startAccessor='checkedDate'
                    endAccessor='offEndDate'
                    events={ props.events }/>
                <Dialog title={ '請假紀錄' }
                        className='rc-dialog-dayoff-header'
                        visible={ props.showDialog } 
                        onClose={ () => { this.props.onDialogClose() } }
                        animation="zoom"
                        maskAnimation="fade"
                        style={{ top: '6%' }}>
                    <ReactTable data={data}
                                columns={columns}/>
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

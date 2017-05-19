import React, { Component } from 'react';
import { connect } from 'react-redux';
import { actionCreators } from '../store/notificationStore';
import ReactTable from 'react-table';
import 'react-table/react-table.css';

var data = [
    {
        isChecked: true,
        UserName: '劉昌衡',
        CheckedDate: '2017-05-05',
        OffTime: '18:00-18:00',
        OffReason: 'OAO'
    }
];
const columns = [
    {
        Header: '',
        accessor: 'isChecked',
        width: 30,
        Cell: props => <input type='checkbox' checked={props.value} onChange={() => props.value = !props.value }/>
    },
    {
        Header: '姓名',
        accessor: 'UserName' 
    }, {
        Header: '請假日期',
        accessor: 'CheckedDate',
        Cell: props => <span className='number'>{props.value}</span>
    }, {
        Header: '請假時間',
        accessor: 'OffTime'
    }, {
        Header: '請假原因',
        accessor: 'OffReason'
    }
];
class Notification extends Component {
    render() {
        return (
            <div style={{width: '85%', margin: '0 auto'}}>
                <ReactTable data={data}
                            columns={columns}
                            defaultPageSize={10}
                            noDataText='無資料!'/>
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {
        data: state.notification
    };
}

const mapDispatchToProps = (dispatch) => {
    return {

    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Notification);

import React, {Component} from 'react';
import {connect} from 'react-redux';
import {actionCreators} from '../store/notificationStore';
import ReactTable from 'react-table';
import 'react-table/react-table.css';
import Dialog from 'rc-dialog';
import 'rc-dialog/assets/index.css';
import Tab from './Tab';

class Notification extends Component {
    constructor(props) {
        super(props);
        this.columns = [
            {
                Header: '姓名',
                accessor: 'userName'
            }, {
                Header: '請假日期',
                accessor: 'checkedDate'
            }, {
                Header: '請假類別',
                accessor: 'offType'
            }, {
                Header: '請假時間',
                accessor: 'offTime'
            }, {
                Header: '請假原因',
                accessor: 'offReason'
            }, {
                Header: '狀態',
                accessor: 'statusOfApproval',
                minWidth: 160,
                Cell: props => {
                    return props.value === '審核中'
                        ? (
                            <div>
                                <button
                                    style={{
                                    padding: '3px 5px'
                                }}
                                    className='btn-2-group btn_date btn_default'
                                    name='approve'
                                    onClick={(e) => this.props.handleClick(props.index, e.target.name)}>同意</button>
                                <button
                                    style={{
                                    padding: '3px 5px'
                                }}
                                    className='btn-2-group btn_date btn_danger'
                                    name='negate'
                                    onClick={(e) => this.props.handleClick(props.index, e.target.name)}>不同意</button>
                            </div>
                        )
                        : (
                            <div>{props.value}</div>
                        );
                }
            }
        ];
        this.selfColumns = [
            {
                Header: '日期',
                accessor: 'checkedDate'
            }, {
                Header: '類別',
                accessor: 'offType'
            }, {
                Header: '時間',
                accessor: 'offTime'
            }, {
                Header: '原因',
                accessor: 'offReason'
            }, {
                Header: '狀態',
                accessor: 'statusOfApproval'
            }, {
                Header: '取消',
                minWidth: 120,
                Cell: props => {
                    return (
                        <button
                            style={{
                            padding: '3px 5px'
                        }}
                            className='btn_date btn_danger'
                            name='delete'
                            onClick={(e) => this.props.handleClick(props.index, e.target.name)}>取消申請</button>
                    );
                }
            }
        ];
        this.otColumns = [
            {
                Header: '姓名',
                accessor: 'userName'
            }, {
                Header: '加班日期',
                accessor: 'checkedDate'
            }, {
                Header: '加班時間',
                accessor: 'overtimeEndTime',
                Cell: props => <span>{`19:00 - ${props.value}`}</span>
            }, {
                Header: '狀態',
                accessor: 'statusOfApprovalForOvertime',
                minWidth: 160,
                Cell: props => {
                    return props.value === '審核中'
                        ? (
                            <div>
                                <button
                                    style={{
                                    padding: '3px 5px'
                                }}
                                    className='btn-2-group btn_date btn_default'
                                    name='approve'
                                    onClick={(e) => this.props.handleOTClick(props.index, e.target.name)}>同意</button>
                                <button
                                    style={{
                                    padding: '3px 5px'
                                }}
                                    className='btn-2-group btn_date btn_danger'
                                    name='negate'
                                    onClick={(e) => this.props.handleOTClick(props.index, e.target.name)}>不同意</button>
                            </div>
                        )
                        : (
                            <div>{props.value}</div>
                        );
                }
            }
        ];
    }
    componentWillMount() {
        this.props.getInitState();
        this.props.getSelfState();
    }

    render() {
        var props = this.props.data;
        var content = props.t >= 0
            ? (
                <div>
                    <p>刪除後不能復原，必須重新申請，是否繼續?</p>
                    <div>
                        <button
                            className='btn_date btn_delete'
                            onClick={() => {
                            this
                                .props
                                .onRemoveRecord(props.t)
                        }}>
                            確定
                        </button>
                    </div>
                </div>
            )
            : (
                <div>
                    取消請假時間已過，若要取消，請通知管理員
                </div>
            );
        const components = [
            {
                title: '請假審核',
                component: <ReactTable
                        data={props.data}
                        columns={this.columns}
                        loading={props.isLoading}
                        defaultPageSize={5}
                        showPageSizeOptions={false}
                        previousText='上一頁'
                        nextText='下一頁'
                        loadingText='載入中...'
                        pageText='第'
                        ofText='之'
                        rowsText='筆'
                        noDataText='無資料!'/>
            }, {
                title: '加班審核',
                component: <ReactTable
                        data={props.otData}
                        columns={this.otColumns}
                        loading={props.OTLoading}
                        defaultPageSize={5}
                        showPageSizeOptions={false}
                        previousText='上一頁'
                        nextText='下一頁'
                        loadingText='載入中...'
                        pageText='第'
                        ofText='之'
                        rowsText='筆'
                        noDataText='無資料!'/>
            }, {
                title: '個人申請查詢',
                component: <ReactTable
                        data={props.selfData}
                        columns={this.selfColumns}
                        loading={props.selfLoading}
                        defaultPageSize={5}
                        showPageSizeOptions={false}
                        previousText='上一頁'
                        nextText='下一頁'
                        loadingText='載入中...'
                        pageText='第'
                        ofText='之'
                        rowsText='筆'
                        noDataText='無資料!'/>
            }
        ];
        return (
            <div>
                <Tab components={components} defaultActiveIndex={0}/>
                <Dialog
                    title={'警告!'}
                    visible={props.showDialog}
                    onClose={() => {
                    this
                        .props
                        .onDialogCancel()
                }}
                    animation="zoom"
                    maskAnimation="fade"
                    style={{
                    top: '30%'
                }}>
                    {content}
                </Dialog>
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {data: state.notification};
}

const mapDispatchToProps = (dispatch) => {
    return {
        getInitState: () => {
            dispatch(actionCreators.getInitState());
        },
        getSelfState: () => {
            dispatch(actionCreators.getSelfState());
        },
        handleClick: (index, val) => {
            if (val === 'delete') 
                dispatch(actionCreators.showDialog(index));
            else 
                dispatch(actionCreators.handleClick(index, val));
            }
        ,
        onDialogCancel: () => {
            dispatch(actionCreators.closeDialog());
        },
        onRemoveRecord: (index) => {
            dispatch(actionCreators.onRemoveRecord(index));
        },
        handleOTClick: (index, name) => {
            dispatch(actionCreators.handleOTClick(index, name));
        }
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Notification);

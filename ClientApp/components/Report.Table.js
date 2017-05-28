import React, { Component } from 'react';
import { connect } from 'react-redux';
import moment from 'moment';
import Select from 'react-select';
import 'react-select/dist/react-select.css';
import { getOptions } from './ManageAccount';
import { actionCreators } from '../store/reportStore';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.min.css';
import { DateInput } from './DayOff';
import ReactTable from 'react-table';
import 'react-table/react-table.css';
import Dialog from 'rc-dialog';
import 'rc-dialog/assets/index.css';

class Table extends Component {
    constructor(props) {
        super(props);
        this.renderTitle = this.renderTitle.bind(this);
        this.renderColumn = this.renderColumn.bind(this);
        this.renderDialog = this.renderDialog.bind(this);
        this.columns = this.renderColumn();
    }

    renderTitle(props) {
        var dopt = '', opt = '', title = '', result;
        switch(props.dateOptions){
            case '0':
                dopt = '今日';
                break;
            case '1':
                dopt = '7日內';
                break;
            case '2':
                dopt = '14日內';
                break;
            case '3':
                dopt = '30日內';
                break;
            case '4':
                dopt = '90日內';
                break;
            case '-1':
                dopt = `${props.fromDate.format('YYYY-MM-DD')} - ${props.toDate.format('YYYY-MM-DD')}`;
                break;
        }
        switch(props.options){
            case '0':
                opt = '所有狀態';
                break;
            case '1':
                opt = '出勤';
                break;
            case '2':
                opt = '請假';
                break;
        }
        if(props.all)
            title = '全體員工 ';
        return opt = `${title}${dopt} ${opt}`;
    }

    renderColumn() {
        var columns = [{
            Header: '姓名',
            accessor: 'userName'
        }, {
            Header: '日期',
            accessor: 'checkedDate'
        }];
        if (this.props.showCheckIn) {
            columns.push({
                Header: '上班時間',
                accessor: 'checkInTime',
            }, {
                Header: '下班時間',
                accessor: 'checkOutTime',
            });
        }
        if (this.props.showGeo) {
            columns.push({
                Header: '上班打卡座標',
                accessor: 'geoLocation1',
            }, {
                Header: '下班打卡座標',
                accessor: 'geoLocation2',
            });
        }
        if (this.props.showOff) {
            columns.push({
                Header: '請假類別',
                accessor: 'offType',
            }, {
                Header: '請假時間',
                accessor: 'offTime',
            }, {
                Header: '請假原因',
                accessor: 'offReason',
            });
        }
        if(this.props.showStatus) {
            columns.push({
                Header: '狀態',
                accessor: 'statusOfApproval',
            });
        }
        columns.push({
            Header: '修改',
            minWidth: 120,
            Cell: props => {
                return (
                    <button style={{padding:'3px 5px'}}
                            className='btn_date btn_danger'
                            onClick={ (e) => this.props.onChangingDataBtnClick(props.index) }>修改</button>
                );
            }
        });
        return columns;
    }

    renderDialog(content) {
        if(!content)
            return null;
        var style = {
            title: {
                fontWeight: '900', textAlign: 'center', marginBottom: '8px'
            },
            label: {
                width: '110px',
                display: 'inline-block',
            },
            input: {
                width: '50%'
            },
            wrapper: {
                marginBottom: '8px'
            }
        };
        var showCheckIn = this.props.showCheckIn ? (
            <div>
                <div style={style.wrapper}>
                    <span style={style.label}>上班時間:</span>
                    <input style={style.input}
                           type='time'
                           name='checkInTime' 
                           value={ content.checkInTime ? content.checkInTime.slice(0, 8) : null }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>下班時間:</span>
                    <input style={style.input}
                           type='time' 
                           name='checkOutTime'
                           value={ content.checkOutTime ? content.checkOutTime.slice(0, 8) : null }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
            </div>
        ) : null;
        var showGeo = this.props.showGeo ? (
            <div>
                <div style={style.wrapper}>
                    <span style={style.label}>上班打卡座標:</span>
                    <input style={style.input}
                           type='text' 
                           name='geoLocation1'
                           value={ content.geoLocation1 }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>下班打卡座標:</span>
                    <input style={style.input}
                           type='text' 
                           name='geoLocation2'
                           value={ content.geoLocation2 }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
            </div>
        ) : null;
        var showOff = this.props.showOff ? (
            <div>
                <div style={style.wrapper}>
                    <span style={style.label}>請假類別:</span>
                    <input style={style.input}
                           type='select' 
                           value={ content.offType }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>請假時間(起):</span>
                    <input style={style.input}
                           type='time' 
                           value={ content.offTimeStart }
                           name='offTimeStart'
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>請假時間(迄):</span>
                    <input style={style.input}
                           type='time' 
                           value={ content.offTimeEnd }
                           name='offTimeEnd'
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>請假原因:</span>
                    <textarea style={style.input}
                              value={ content.offReason }
                              name='offReason'
                              onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
            </div>
        ) : null;
        var showStatus = this.props.showStatus ? (
            <div>
                <span style={style.label}>狀態:</span>
                <input style={style.input}
                       type='select' 
                       value={ content.statusOfApproval }/>
            </div>
        ) : null;
        return (
            <div className='selectReport'>
                <div style={ style.title }>{ content.userName }</div>
                <div style={ style.title }>
                    { new moment(content.checkedDate, 'YYYY-MM-DD').format('YYYY年MM月DD日') }
                </div>
                { showCheckIn }
                { showGeo }
                { showOff }
                { showStatus }
                <div>
                    <button className='btn_date btn_date_group btn_default'>確認修改</button>
                    <button className='btn_date btn_date_group btn_danger'>刪除紀錄</button>
                    <button className='btn_date btn_date_group btn_info'
                            onClick={ () => this.props.onDialogClose() }>取消</button>
                </div>
            </div>
        );
    }

    render() {
        var props = this.props.data;
        var content = props.t >= 0 ? props.data[props.t] : null;
        var contentInDialog = this.renderDialog(content);
        var showPageSizeOptions = true;
        if(typeof window === 'object') {
            if(window.innerWidth < 600)
                showPageSizeOptions = false;
        }
            
        return (
            <div style={{textAlign: 'left', width: '90%', margin: '0 auto'}}>
                <h3 style={{marginBottom: '10px'}}>{ this.renderTitle(props) }</h3>
                <ReactTable data={props.data}
                            columns={this.columns}
                            loading={props.isLoading}
                            defaultPageSize={5}
                            showPageSizeOptions={ showPageSizeOptions }
                            previousText= '上一頁'
                            nextText= '下一頁'
                            loadingText= '載入中...'
                            pageText= '第'
                            ofText= '之'
                            rowsText= '筆'
                            noDataText='無資料!'/>
                <div style={{width: '150px', margin: '0 auto', marginTop: '16px'}}>
                    <button className='btn_date btn_default' onClick={ () => this.props.query() }>匯出</button>
                </div>
                <Dialog title={'修改資料!'} 
                            visible={ props.showDialog } 
                            onClose={ () => this.props.onDialogClose() }
                            animation="zoom"
                            maskAnimation="fade"
                            style={{ top: '6%' }}>
                        { contentInDialog }
                </Dialog>
            </div>
        );
    }
}
const mapStateToProps = (state) => {
    return {
        data: state.report
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        onChangingDataBtnClick: (e) => {
            return dispatch(actionCreators.onChangingDataBtnClick(e));
        },
        onDialogClose: () => {
            return dispatch(actionCreators.onDialogClose());
        },
        onInputChange: (val, name) => {
            return dispatch(actionCreators.onInputChange(val, name));
        }
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Table);


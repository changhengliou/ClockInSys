import React, { Component } from 'react';
import { connect } from 'react-redux';
import moment from 'moment';
import Select from 'react-select';
import 'react-select/dist/react-select.css';
import { getOptions } from './ManageAccount';
import { actionCreators, getDateOptions } from '../store/reportStore';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.min.css';
import { DateInput } from './DayOff';
import ReactTable from 'react-table';
import 'react-table/react-table.css';
import Dialog from 'rc-dialog';
import 'rc-dialog/assets/index.css';
import DialogContent from './Report.Dialog';

class Table extends Component {
    constructor(props) {
        super(props);
        this.renderTitle = this.renderTitle.bind(this);
        this.renderColumn = this.renderColumn.bind(this);
    }

    
    componentWillMount() {
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
        var val = this.props.data.data;
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
                accessor: 'offTimeStart',
                Cell: props => {
                    return (
                        <span>
                            { val[props.index] ? val[props.index].offTimeStart ? 
                              `${val[props.index].offTimeStart} - ${val[props.index].offTimeEnd}` :
                               null : null }
                        </span>
                    );
                }
            }, {
                Header: '請假原因',
                accessor: 'offReason',
            });
        }
        if(this.props.showStatus) {
            columns.push({
                Header: '狀態',
                accessor: 'statusOfApproval',
                Cell: props => {
                    return (
                    <span>
                        { val[props.index] ? val[props.index].offType ? props.value : null : null }
                    </span>);
                }
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

    render() {
        var props = this.props.data;
        var showPageSizeOptions = true;
        if(typeof window === 'object') {
            if(window.innerWidth < 600)
                showPageSizeOptions = false;
        }
            
        return (
            <div style={{textAlign: 'left', width: '90%', margin: '0 auto'}}>
                <h3 style={{marginBottom: '10px'}}>{ this.renderTitle(props) }</h3>
                <div style={{verticalAlign: 'middle', textAlign: 'left', marginBottom: '10px'}}>
                    <div style={{width: '100px', display: 'inline-block', marginRight:'2%'}}>
                        <DatePicker dropdownMode="select"
                            showMonthDropdown showYearDropdown
                            customInput={<DateInput/>}
                            onChange={ this.props.onDateChange } 
                            selected={ new moment(props.date, 'YYYY-MM-DD') }/>
                    </div>
                    <div style={{width: '36%', display: 'inline-block', height: '22px', marginRight:'2%'}}>
                        <Select.Async value={ props.name }
                              onChange={ this.props.handleNameChange }
                              ignoreCase={ true }
                              loadOptions={ getOptions }/>
                    </div>
                    <button className='btn_date btn_default' 
                            style={{display: 'inline-block', width: '100px'}}
                            onClick={ () => {
                                if(props.name)
                                    this.props.onNewRecord()
                            } }>新增紀錄</button>
                </div>
                <ReactTable data={ props.data }
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
                    <button className='btn_date btn_default' onClick={ () => { 
                        var id = props.all ? '' : props.id,
                            options = props.options,
                            fromDate = props.dateOptions === '-1' ?
                                       props.fromDate.format('YYYY-MM-DD') : 
                                       getDateOptions(props.dateOptions),
                            toDate = props.dateOptions === '-1' ?
                                     props.toDate.format('YYYY-MM-DD') : 
                                     new moment().format('YYYY-MM-DD');
                        window.open(`/api/record/exportXLSX?a=${id}&b=${options}&c=${fromDate}&d=${toDate}`,'_blank')
                    } }>匯出</button>
                </div>
                <Dialog title={'修改資料!'} 
                            visible={ props.showDialog } 
                            onClose={ () => this.props.onDialogClose() }
                            animation="zoom"
                            maskAnimation="fade"
                            style={{ top: '6%' }}>
                        <DialogContent s={ props.s }
                                       a={ props.a }
                                       f={ props.f }
                                       showCheckIn={ this.props.showCheckIn }
                                       showGeo={ this.props.showGeo }
                                       showOff={ this.props.showOff }
                                       showStatus={ this.props.showStatus }
                                       showBtn={ this.props.showBtn }
                                       onInputChange={ this.props.onInputChange }
                                       onSubmitReport={ this.props.onSubmitReport }
                                       onDeleteReport={ this.props.onDeleteReport }
                                       onDialogClose={ this.props.onDialogClose }/>
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
        },
        onSubmitReport: () => {
            return dispatch(actionCreators.onSubmitReport());
        },
        onDeleteReport: () => {
            return dispatch(actionCreators.onDeleteReport());
        },
        handleNameChange: (val) => {
            var z = null;
            if(val)
                z = val.value;
            return dispatch(actionCreators.handleNameChange(z));
        },
        onDateChange: (val) => {
            return dispatch(actionCreators.onDateChange(val));
        },
        onNewRecord: () => {
            return dispatch(actionCreators.onNewRecord());
        }
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Table);


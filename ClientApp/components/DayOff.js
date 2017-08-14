import React, { Component } from 'react';
import DatePicker from 'react-datepicker';
import { connect } from 'react-redux';
import moment from 'moment';
import { actionCreators } from '../store/dayoffStore';
import 'react-datepicker/dist/react-datepicker.min.css';
import '../css/manageAccount.css';
import '../css/dayoff.css';
import OffOption from './DayOff.Options';

export class DateInput extends Component {
    render() {
        var style = { height: '34px' };
        if(this.props.disabled)
            style = { backgroundColor: '#ccc' };
        return (
            <button className='btn_date' onClick={this.props.onClick} style={style}>
                {this.props.value}
            </button>
        );
    }
}

DateInput.propTypes = {
    value: React.PropTypes.string,
    onClick: React.PropTypes.func
};

const style = {
    label: { width: '160px' },
    textarea: { height: '60px' },
    select : { textAlign: 'center', width: '43%' },
    type: { textAlign: 'center' },
    disabled : { backgroundColor: '#ccc', borderColor: '#bbb' }
};

// this.props.disableToDate, disableOffType, disableFromTime, disableToTime, disableOffReason
class DayOff extends Component {
    constructor(props) {
        super(props);
        this.renderHourOptions = this.renderHourOptions.bind(this);
    }

    renderHourOptions(isToday = false) {
        var time = new Date();
        var hours = time.getHours() + 1;
        var hourOption = isToday ? [] : 
                                   [9, 10, 11, 12, 13, 14, 15, 16, 17, 18];
        var options = [];
        if (isToday) {
            for(var i = hours; i <= 18; i++) {
                hourOption.push(i);
            } 
        }
        hourOption.map((obj) => {
            options.push(<option value={obj}>{obj}:00</option>);
        });
        return options;
    }
    
    render() {
        var isToday = false,
            diffDate = false,
            props = this.props.data,
            timeDiff = (props.toTime - props.fromTime) / 8,
            fromDate = new moment(props.fromDate, 'YYYY-MM-DD'),
            toDate = new moment(props.toDate, 'YYYY-MM-DD'),
            disabledDate = props.disabledContent === 'none' || props.disabledContent === 'partial'
             ? false : true,
            disableToDate = this.props.disableToDate;
            
        if (timeDiff > 1)
            timeDiff = 1;
        if (props.fromDate !== props.toDate) {
            diffDate = true;
        } else {
            if (new moment(props.fromDate, 'YYYY-MM-DD').isSame(new moment(), 'days'))
                isToday = true;
        }
        return (
            <div>
                <div style={{margin: '0px auto', width: '60%', textAlign: 'left'}}>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>(起)時間:</label>
                        <DatePicker id="from" selectsStart dropdownMode="select"
                            showMonthDropdown disabled
                            minDate={ fromDate }
                            customInput={<DateInput disabled/>} 
                            selected={ fromDate } 
                            startDate={ fromDate }
                            endDate={ toDate }/>
                    </div>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>(迄)時間:</label>
                        <DatePicker id="to" selectsEnd dropdownMode="select"
                        showMonthDropdown disabled={ disableToDate }
                        minDate={ fromDate }
                        customInput={<DateInput disableToDate/>} 
                        onChange={ (e) => { this.props.onDateChange(e) } }
                        selected={ toDate } 
                        startDate={ fromDate }
                        endDate={ toDate }/>
                    </div>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>種類:</label>
                        <OffOption className='dayOffInput'
                                   disabledAll={ this.props.disableOffType }
                                   value={ props.offType }
                                   sickLeaves={ props.s }
                                   sickLeavesOnly={ this.props.sickLeavesOnly }
                                   annualLeaves={ props.a }
                                   familyCareLeaves={ props.f }
                                   onChange={ (e) => this.props.onOffTypeChange(e.target.value) }/>
                    </div>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>期間:
                            <font style={{color: 'red', marginLeft: '10px'}}>{ timeDiff ? `${timeDiff}天` : '無效的時間'}</font>
                        </label>
                        <div>
                            <select name="fromTime" className="dayOffInput" disabled={ diffDate || this.props.disableFromTime }
                                    value={ props.fromTime }
                                    style={ this.props.disableFromTime || diffDate ? { ...style.select, ...style.disabled } : style.select } 
                                    disabled={ this.props.disableFromTime || diffDate }
                                    onChange={ (e) => this.props.onTimeChange(e.target.name, e.target.value) }>
                                {
                                    diffDate ? <option value='9'>9:00</option> : 
                                               this.renderHourOptions(isToday).map((i) => {
                                                   return i;
                                               })
                                }
                            </select>
                            <label style={{fontSize: '20px', color: '#4a4a4a', width: '14%', textAlign: 'center', display: 'inline-block'}}>至</label>
                            <select name="toTime" className="dayOffInput" 
                                    value={ props.toTime }
                                    style={ diffDate || this.props.disableToTime ? { ...style.select, ...style.disabled } : style.select } 
                                    disabled={ this.props.disableToTime || diffDate }
                                    onChange={ (e) => this.props.onTimeChange(e.target.name, e.target.value) }>
                                {
                                    diffDate ? <option value='18'>18:00</option> : 
                                               this.renderHourOptions(isToday).map((i) => {
                                                   return i;
                                               })
                                }
                            </select>
                        </div>
                    </div>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>請假事由:</label>
                        <textarea id="offReason" 
                                  maxLength="100"
                                  disabled={ this.props.disableOffReason }
                                  value={ props.offReason }
                                  onChange={ (e) => this.props.onOffReasonChange(e.target.value) }
                                  style={ this.props.disableOffReason ? { ...style.textarea, ...style.disabled } : style.textarea }
                                  className="dayOffInput"></textarea>
                    </div>
                    <div className="dialog_row">
                        <button className="btn-2-group btn_date btn_info" disabled={ !timeDiff || this.props.disableConfirmBtn ? true : false }
                                style={ !timeDiff || this.props.disableConfirmBtn ? style.disabled : null }
                                onClick={ () => { this.props.onDialogConfirm() } }>{ disabledDate ? '修改' : '確認' }</button>
                        <button className={ this.props.isBtnCancel ? 'btn_delete btn-2-group btn_date' : 'btn_trans_blue btn-2-group btn_date'}
                                onClick={ () => {
                                    if(this.props.isBtnCancel)
                                        this.props.onCancelLeaves(props.fromDate)
                                    else
                                        this.props.onDialogClose()
                                } }>
                                { this.props.isBtnCancel ? '取消請假' : '關閉' }
                        </button>
                    </div>
                </div>
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
        onDateChange: (date) => {
            dispatch(actionCreators.onDateChange(date.format('YYYY-MM-DD')));
        },
        onTimeChange: (name, value) => {
            dispatch(actionCreators.onTimeChange(name, value));
        },
        onOffTypeChange: (value) => {
            dispatch(actionCreators.onOffTypeChange(value));
        },
        onOffReasonChange: (value) => {
            dispatch(actionCreators.onOffReasonChange(value));
        },
        onDialogOpen: () => {
            dispatch(actionCreators.onDialogOpen());
        },
        onDialogClose: () => {
            dispatch(actionCreators.onDialogClose());
        },
        onDialogConfirm: () => {
            dispatch(actionCreators.onDialogConfirm());
        },
        onCancelLeaves: (date) => {
            dispatch(actionCreators.onCancelLeaves(date));
        }
    }
}
const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(DayOff);

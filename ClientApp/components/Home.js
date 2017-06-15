import * as React from 'react';
import { connect } from 'react-redux';
import { actionCreators, reducer } from '../store/homeStore';
import '../css/index.css';
import moment from 'moment';
import { Link } from 'react-router';
import Dialog from 'rc-dialog';
import 'rc-dialog/assets/index.css';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.min.css';
import { DateInput } from './DayOff';

const style = {
    tr1: { textAlign: 'left', marginLeft: '2em' },
    tr2: { textAlign: 'left', paddingLeft: '2em'}
}

class Home extends React.Component {
    constructor(props) {
        super(props);
        this.intervalId;
        this._intervalId;
        this.init = this.init.bind(this);
    }

    init() {
        this.props.getInitState();
        var map = new google.maps.Map(document.getElementById('map'), {
            center: {
                lat: 24.997233,
                lng: 121.538548
            },
            zoom: 13
        });
        var infoWindow = new google.maps.InfoWindow({
            map: map
        });
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function(position) {
                var pos = {
                    lat: position.coords.latitude,
                    lng: position.coords.longitude
                };
                infoWindow.setPosition(pos);
                infoWindow.setContent(pos.lat.toString().substring(0, 9) + ", " + pos.lng.toString().substring(0, 9));
                map.setCenter(pos);
                window.pos = pos;
            },
            function() {
                handleLocationError(true, infoWindow, map.getCenter());
            });
        } else {
            handleLocationError(false, infoWindow, map.getCenter());
        }
        
        window.handleLocationError = (browserHasGeolocation, infoWindow, pos) => {
            infoWindow.setPosition(pos);
            infoWindow.setContent(browserHasGeolocation ? '定位錯誤' :
                '不支援定位的瀏覽器');
        }
    }

    componentDidMount() {
        if (typeof window === 'object') {
            if (window.onload)
                window.onload();
            window.onload = () => {
                if (typeof google === 'object')
                    this.init();
            }
            this._intervalId = setInterval(() => {
                if (this.props.data.currentTime) {
                    var time = new moment(this.props.data.currentTime, "HH:mm:ss tt");
                    this.intervalId = setInterval(() => {
                        this.props.timeTicking(time);
                    }, 1000);
                    clearInterval(this._intervalId);
                }
            }, 200);
            setTimeout(() => {clearInterval(this._intervalId)}, 10000);
        }
    }
    
    componentWillUnmount() {
        clearInterval(this.intervalId);
        clearInterval(this._intervalId);
    }
    
    render() {
        var data = this.props.data;
        var disabledIn = data.shouldCheckInDisable ? 'disabled' : '';
        var disabledOut = data.shouldCheckOutDisable ? 'disabled' : '';
        var checkIn = data.checkIn ? new moment(data.checkIn, 'HH:mm:ss tt').format("HH:mm:ss") : null;
        var checkOut = data.checkOut ? new moment(data.checkOut, 'HH:mm:ss tt').format("HH:mm:ss") : null;
        return (
            <div>
                <div className="content_wrapper">
                    <div id="dispDate" className="checkin_date">{ data.currentDate }</div>
                    <div id="dispTime" className="checkin_time">{ data.currentTime }</div>
                    <table
                        style={{
                        width: '50%',
                        margin: '0 auto',
                        marginBottom: '3%'}}>
                        <tbody>
                            <tr>
                                <td style={style.tr1} className="record_time">上班:</td>
                                <td style={style.tr2} className="record_time">{ checkIn }</td>
                            </tr>
                            <tr>
                                <td style={style.tr1} className="record_time">下班:</td>
                                <td style={style.tr2} className="record_time">{ checkOut }</td>
                            </tr>
                            <tr>
                                <td style={style.tr1} className="record_time">本日狀態:</td>
                                <td style={style.tr2} className="record_time">
                                    { data.offStatus ? data.offStatus : null }
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className='map_wrapper'>
                    <div id='map' style={{height: '100%', width: '100%'}}>請刷新瀏覽器</div>
                </div>
                <div className="content_wrapper">
                    <div style={{marginBottom: '14px'}}>
                        <button id="checkIn" className={`btn_date btn_blue ${disabledIn}`} 
                                disabled={data.shouldCheckInDisable}
                                style={{width: '23%', marginRight: '5%', marginLeft: '4%'}}
                                onClick={() => {this.props.proceedCheck('checkIn');}}>
                                簽到
                        </button>
                        <button id="checkOut" className={`btn_date btn_blue ${disabledOut}`}
                                disabled={data.shouldCheckOutDisable}
                                style={{width: '23%', marginRight: '5%'}}
                                onClick={() => {this.props.proceedCheck('checkOut');}}>
                                簽退
                        </button>
                    </div>
                    <button className="btn_date btn_danger"
                            style={{width: '23%', marginRight: '5%', marginLeft: '4%'}}>
                            <Link to='/dayOff' style={{color: '#fff', textDecoration: 'none', display: 'block'}}>
                                請假
                            </Link>
                    </button>
                    <button className="btn_date btn_violet"
                            style={{width: '23%', marginRight: '5%'}}
                            onClick={() => this.props.onOpenOTDialog() }>
                            加班申請
                    </button>
                </div>
                <Dialog title='加班申請' 
                        className='rc-dialog-dayoff-header'
                        visible={ this.props.showOTDialog } 
                        style={{ top: '20%', textAlign: 'center' }}
                        animation="zoom"
                        maskAnimation="fade"
                        onClose={ () => this.props.onCloseOTDialog() }>
                    <div style={{margin: '0px auto', width: '60%', textAlign: 'left'}}>
                        <div style={{marginBottom: '12px', textAlign: 'left'}}>
                            <label style={{display: 'inline-block', width: '60px'}}>日期:</label>
                            <DatePicker dropdownMode="select"
                                        showMonthDropdown
                                        onChange={ (e) => this.props.onOTDateChange(e) }
                                        minDate={ new moment() }
                                        maxDate={ new moment().add(30, 'days') }
                                        customInput={ <DateInput/> }
                                        selected={ new moment(this.props.OTDate, 'YYYY-MM-DD') }/>
                        </div>
                        <div style={{marginBottom: '12px', textAlign: 'left'}}>
                            <label style={{display: 'block', width: '60px'}}>時間:</label>
                            <input style={{height: '34px', background: '#ccc', 
                                           border: '1px solid #aaa', textAlign: 'center',
                                           width: '43%'}} 
                                   value='19:00' disabled/>
                            <label style={{fontSize: '20px', color: '#4a4a4a', width: '14%', textAlign: 'center', display: 'inline-block'}}>至</label>
                            <select style={{height: '34px', textAlign: 'center', width: '43%'}} 
                                    value={this.props.OTTime} onChange={(e) => this.props.onOTTimeChange(e.target.value) }>
                                    <option value='19:30'>19:30</option>
                                    <option value='20:00'>20:00</option>
                                    <option value='20:30'>20:30</option>
                                    <option value='21:00'>21:00</option>
                                    <option value='21:30'>21:30</option>
                                    <option value='22:00'>22:00</option>
                                    <option value='22:30'>22:30</option>
                                    <option value='23:00'>23:00</option>
                                    <option value='23:30'>23:30</option>
                            </select>
                        </div>
                        <div>
                            <button className='btn_date btn_info' style={{width: '40%', margin: '0 3.5% 0 2.5%'}}
                                    onClick={ () => this.props.onCloseOTDialog() }>確定</button>
                            <button className='btn_date btn_danger' style={{width: '40%', margin: '0 1.5% 0 8.5%'}}
                                    onClick={ () => this.props.onCloseOTDialog() }>取消</button>
                        </div>
                    </div>
                </Dialog>
                <Dialog title='打卡完成' 
                        className='rc-dialog-dayoff-header'
                        visible={ this.props.showDialog } 
                        style={{ top: '40%', textAlign: 'center' }}
                        animation="zoom"
                        maskAnimation="fade"
                        onClose={ () => this.props.onCloseDialog() }>
                    <button className='btn_date btn_danger' style={{width: '30%'}}
                            onClick={ () => this.props.onCloseDialog() }>關閉</button>
                </Dialog>
                <script async defer src="https://maps.googleapis.com/maps/api/js?key=AIzaSyCgR4Fmw4SAbpzAwA6mivcy6viFm38OztE"></script>
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {
        data: state.home.data,
        showDialog: state.home.showDialog,
        showOTDialog: state.home.showOTDialog,
        OTDate: state.home.OTDate,
        OTTime: state.home.OTTime
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        getInitState: () => {
            dispatch(actionCreators.getInitState());
        },
        timeTicking: (time) => {
            dispatch(actionCreators.timeTicking(time))
        },
        proceedCheck: (type) => {
            var position = null;
            if(typeof window === 'object' && window.pos)
                position = `${window.pos.lat}, ${window.pos.lng}`
            dispatch(actionCreators.proceedCheck(position, type));
        },
        onCloseDialog: () => {
            dispatch(actionCreators.onCloseDialog());
        }, 
        onOpenOTDialog: () => {
            dispatch(actionCreators.onOpenOTDialog());
        },
        onCloseOTDialog: () => {
            dispatch(actionCreators.onCloseOTDialog());
        },
        onOTDateChange: (e) => {
            dispatch(actionCreators.onOTDateChange(e));
        },
        onOTTimeChange: (e) => {
            dispatch(actionCreators.onOTTimeChange(e));
        }
    };
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);

export default wrapper(Home);

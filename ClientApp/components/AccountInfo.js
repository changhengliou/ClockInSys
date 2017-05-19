import React, { Component } from 'react';
import '../css/accountInfo.css';
import { connect } from 'react-redux';
import { actionCreators, reducer } from '../store/accInfoStore';


class AccountInfo extends Component {
    constructor(props) {
        super(props);
    }
    
    componentWillMount() {
        this.props.getInitState();
    }
    
    render() {
        var props = this.props.info;
        var deputy = Object.prototype.toString.call(props.deputyName) === '[object Array]'
                      ? props.deputyName.join() : '無';
        var supervisor = Object.prototype.toString.call(props.deputyName) === '[object Array]'
                         ? props.supervisorName.join() : '無';
        if(props.loadingCompleted) 
        return (
            <div>
                <table className="table_details" style={{width: '90%', margin: '0 auto'}}>
                    <thead>
                        <tr><th style={{width: '45%'}}/><th style={{width: '55%'}}/></tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>姓名：</td>
                            <td>{ props.userName }</td>
                        </tr>
                        <tr>
                            <td>入職日：</td>
                            <td>{ props.dateOfEmployment }</td>
                        </tr>
                        <tr>
                            <td>職稱:</td>
                            <td>{ props.jobTitle }</td>
                        </tr>
                        <tr>
                            <td>電子信箱:</td>
                            <td>{ props.userEmail }</td>
                        </tr>
                        <tr>
                            <td>電話:</td>
                            <td>{ props.userPhone }</td>
                        </tr>
                        <tr>
                            <td>特休:</td>
                            <td>剩餘 { props.annualLeaves } 天</td>
                        </tr>
                        <tr>
                            <td>病假:</td>
                            <td>剩餘 { props.sickLeaves } 天</td>
                        </tr>
                        <tr>
                            <td>家庭照顧假:</td>
                            <td>剩餘 { props.familyCareLeaves } 天</td>
                        </tr>
                        <tr>
                            <td>代理人：</td>
                            <td>{ deputy }</td>
                        </tr>
                        <tr>
                            <td>主管：</td>
                            <td>{ supervisor }</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        );
        else
            return (<div style={{fontSize: '24px'}}>載入中...</div>);
    }
}

const mapStateToProps = (state) => {
    return {
        info: state.__info__
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        getInitState: () => {
            dispatch(actionCreators.getInitState());
        }
    };
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);

export default wrapper(AccountInfo);
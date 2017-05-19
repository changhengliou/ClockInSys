import React, { Component } from 'react';
import { connect } from 'react-redux';
import Select from 'react-select';
import 'react-select/dist/react-select.css';
import { actionCreators, reducer } from '../store/manageAccStore';
import { fetch } from 'domain-task';
import Dialog from 'rc-dialog';
import 'rc-dialog/assets/index.css';
import '../css/manageAccount.css';
import ManageForm from './ManageAccount.Form';

class ManageAccount extends Component {
    constructor(props) {
        super(props);
        this.getOptions = this.getOptions.bind(this);
    }

    getOptions(input, callback) {
        if(!input) {
            callback(null, {
                options: []
            });
            return;
        }
        let fetchTask = fetch('api/account/getNameList', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Accept': 'application/json, text/javascript, */*; q=0.01',
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                Param: input
            })
        }).then(response => response.json()).then(data => {
            return { options: data.payload };
        }).catch(error => {
            console.log(error);
        });
        return fetchTask;
    }

    render() {
        var props = this.props.data;
        var choosedOpt = props.choosedOpt ? props.choosedOpt.label : null;
        var errorMsg = props.showErrorMsg ? <div className='errmsg'>請選擇帳號!</div> : null;
        if(!props.showContent)
            return (
                <div className='selectManage'>
                    <Select.Async value={ props.choosedOpt }
                              onChange={ this.props.handleChange }
                              ignoreCase={ true }
                              loadOptions={ this.getOptions }/>
                    <div>
                        <button className='btn_date btn_date_group btn_info'
                                onClick={ () => {
                                    if(choosedOpt)
                                        this.props.onUpdateAccountClick(props.choosedOpt.value);
                                    else
                                        this.props.showErrorMsg(props.showErrorMsg) } }>修改帳號</button>
                        <button className='btn_date btn_date_group btn_default'
                                onClick={ () => { this.props.onNewAccountClick() } }>新增帳號</button>
                        <button className='btn_date btn_date_group btn_danger'
                                onClick={ () => { 
                                    if(choosedOpt)
                                        this.props.onDeleteClick()
                                    else
                                        this.props.showErrorMsg(props.showErrorMsg) } }>刪除帳號</button>
                    </div>
                    { errorMsg }
                    <Dialog title={'警告!'} 
                            visible={ props.showDialog } 
                            onClose={ () => { this.props.onDialogCancel() } }
                            animation="zoom"
                            maskAnimation="fade"
                            style={{ top: '30%' }}>
                        <p>
                            你即將要刪除帳號<strong>{ choosedOpt }</strong>，
                            刪除後該帳號之所有紀錄都將清除，這是一個不能還原的動作，確定刪除?
                        </p>
                        <div>
                            <button className='btn_date btn_delete' 
                                     onClick={ () => 
                                     { this.props.onDeleteAccount(props.choosedOpt.value) } }>
                                     確定
                            </button>
                        </div>
                    </Dialog>
                </div>
            );
        else
            return (
                <ManageForm></ManageForm>
            )
    }
}
const mapStateToProps = (state) => {
    return {
        data: state.manageAccount,
    };
}
const mapDispatchToProps = (dispatch) => {
    return {
        handleChange: (value) => {
            dispatch(actionCreators.handleChange(value));
        },
        onNewAccountClick: () => {
            dispatch(actionCreators.onNewAccountClick());
        },
        onUpdateAccountClick: (id) => {
            dispatch(actionCreators.onUpdateAccountClick(id));
        },
        onDeleteAccount: (id) => {
            dispatch(actionCreators.onDeleteAccount(id));
        },
        onDeleteClick: () => {
            dispatch(actionCreators.onDeleteClick());
        },
        onDialogCancel: () => {
            dispatch(actionCreators.onDialogCancel());
        },
        showErrorMsg: (val) => {
            if (!val)
                dispatch(actionCreators.showErrorMsg());
        }
    };
}
export default connect(mapStateToProps, mapDispatchToProps)(ManageAccount);

export const getOptions = ManageAccount.prototype.getOptions;
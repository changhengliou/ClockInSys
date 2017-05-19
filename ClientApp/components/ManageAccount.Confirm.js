import React, { Component } from 'react';
import '../css/manageAccount.css';
import { Link } from 'react-router';
class Confirm extends Component {
    constructor(props) {
        super(props);
        this.msg;
    }
    
    componentWillMount() {
        switch(this.props.params.msg) {
            case 'created':
                this.msg = '創建成功!';
                break;
            case 'createdError':
                this.msg = '創建失敗!';
                break;
            case 'updated':
                this.msg = '更新成功!';
                break;
            case 'updatedError':
                this.msg = '更新失敗!';
                break;
            case 'deleted':
                this.msg = '刪除成功!';
                break;
            case 'deletedError':
                this.msg = '刪除失敗!(不可刪除自己的帳號!)'
            default:
                break;
        }
    }
    
    render() {
        return (
            <div className='selectManage'>
                <div style={{color: 'red', fontWeight: '700', fontSize: '24px', marginBottom: '20px'}}>
                    { this.msg }
                </div>
                <Link to='/manageaccount'>
                    <button className='btn_date btn_default' style={{color: '#fff'}}>
                        返回
                    </button>
                </Link>
            </div>
        );
    }
}

export default Confirm;
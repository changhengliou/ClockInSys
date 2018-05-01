import React, { Component } from 'react';
import { Link } from 'react-router';

class Result extends Component {
    constructor(props) {
        super(props);
    }
    
    render() {
        var newCount = 0, count = 0;
        if (this.props.params.newCount)
            newCount = this.props.params.newCount;
        if(this.props.location.query && this.props.location.query.count)
            count = this.props.location.query.count
        return (
            <div className='selectManage'>
                <div style={{fontWeight: '700', fontSize: '24px', marginBottom: '20px'}}>
                    共 { newCount } 筆資料，成功 { count } 筆
                </div>
                <Link to='/bulkyUpdate'>
                    <button className='btn_date btn_default' style={{color: '#fff'}}>
                        返回
                    </button>
                </Link>
            </div>
        );
    }
}

export default Result;
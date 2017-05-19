import * as React from 'react';
import { Link } from 'react-router';
import { UrlMapping } from '../routes';
import { connect } from 'react-redux';

class NavMenu extends React.Component {
    constructor(props) {
        super(props);
        this.openNav = this.openNav.bind(this);
        this.closeNav = this.closeNav.bind(this);
    }

    openNav() {
        if(typeof document === 'object')
            document.getElementById("mySidenav").style.width = "300px";
    }

    closeNav() {
        if(typeof document === 'object')
            document.getElementById("mySidenav").style.width = "0";
    }

    render() {
        var info = this.props.info;
        return (
            <div>
                <div id="mySidenav" className="mps_left_nav">
                    <div className="header">
                        <div className="email_add">{ info.userEmail }</div>
                        <div className="side_header_title">{ info.userName }</div>
                        <div className="header_close_btn" onClick={this.closeNav}>
                            <i className="fa fa-chevron-left" aria-hidden="true"></i>
                        </div>
                    </div>
                    <div className="left_nav_function">
                        <ul className="left_nav_function" id="admin_function">
                            {
                                Object.keys(UrlMapping).map((obj, index) => {
                                    return (
                                        <li onClick={this.closeNav} key={index}>
                                            <Link to={obj}>{UrlMapping[obj]}</Link>
                                        </li>
                                    );
                                })
                            }
                        </ul>
                        <button onClick={this.closeNav} className="btn_logout">
                            <a href='/account/signout'>登出</a>
                        </button>
                    </div>
                </div>
                <nav className="mps_top_nav">
                    <div className="top_hamburger" onClick={this.openNav}></div>
                    <Link to="/">
                        <div className="system_name">穎哲資訊差勤系統</div>
                    </Link>
                    <div className="nav_top_right">
                        {
                            Object.keys(UrlMapping).map((obj, index) => {
                                return (
                                    <span className="nav_top_right_function" key={index}>
                                        <Link to={obj}>{UrlMapping[obj]}</Link>
                                    </span>
                                );
                            })
                        }
                        <span>{ info.userName }</span>
                        <span className="top_right_bar"></span>
                        <span className="nav_top_right_logout">
                            <a href='/account/signout'>登出</a>
                        </span>
                    </div>
                </nav>
                <div className="header" onClick={this.closeNav}>
                    <div className="header_title">
                        { 
                            UrlMapping[this.props.pathname] ?
                            UrlMapping[this.props.pathname] :
                            UrlMapping['/' + this.props.pathname.split('/')[1]]
                        }
                    </div>
                </div>
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {
        info: state.__info__
    };
}

const wrapper = connect(mapStateToProps);

export default wrapper(NavMenu);
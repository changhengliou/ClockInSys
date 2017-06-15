import * as React from 'react';
import { Link } from 'react-router';
import { UrlMapping, ProtectedUrlMapping } from '../routes';
import { connect } from 'react-redux';
import { actionCreators } from '../store/index';
class NavMenu extends React.Component {
    constructor(props) {
        super(props);
        this.openNav = this.openNav.bind(this);
        this.closeNav = this.closeNav.bind(this);
    }

    openNav() {
        this.props.onNavBarOpen();
    }

    closeNav() {
        this.props.onNavBarClose();
    }

    render() {
        var info = this.props.info;
        var protectedUrl = ProtectedUrlMapping;
        if (info.roles[0] === 'default')
            protectedUrl = {};
        return (
            <div>
                <div id="mySidenav" className="mps_left_nav" style={{ width: this.props.style }}>
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
                            {
                                Object.keys(protectedUrl).map((obj, index) => {
                                    return (
                                        <li onClick={this.closeNav} key={index}>
                                            <Link to={obj}>{protectedUrl[obj]}</Link>
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
                        <div className="system_name">穎哲資訊出勤管理系統</div>
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
                        {
                            Object.keys(protectedUrl).map((obj, index) => {
                                return (
                                    <span className="nav_top_right_function" key={ index * -1 }>
                                        <Link to={obj}>{protectedUrl[obj]}</Link>
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
                        { 
                            protectedUrl[this.props.pathname] ?
                            protectedUrl[this.props.pathname] :
                            protectedUrl['/' + this.props.pathname.split('/')[1]]
                        }
                    </div>
                </div>
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {
        info: state.__info__,
        style: state.__w3x__.__s__
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        onNavBarOpen: () => {
            dispatch(actionCreators.onNavBarOpen());
        },
        onNavBarClose: () => {
            dispatch(actionCreators.onNavBarClose());
        }
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);

export default wrapper(NavMenu);
/* eslint-disable @shopify/jsx-no-complex-expressions */
import {
  Box,
  CircularProgress,
  Divider,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  ListSubheader,
  styled,
  Typography,
  useTheme,
} from '@mui/material';
import {
  HomeOutlined,
  StorefrontOutlined,
  WalletOutlined,
} from '@mui/icons-material';
import {NavLink, useLocation} from 'react-router-dom';
import {useTranslation} from 'react-i18next';
import {useQuery} from 'react-query';

import {MenuItem} from './components';

import {getTokenBalance} from '@/data/api/requests/wallet';
import IWalletsBalance from '@/data/api/types/IWalletsBalance';
import {routeHome, routeMarket, routeWallet} from '@/pages/routes';

export const walletBalanceQuery = 'walletsBalance';

function SideBar() {
  const {t} = useTranslation();
  const location = useLocation();
  const theme = useTheme();

  const {data, status} = useQuery<IWalletsBalance>(
    'walletsBalance',
    getTokenBalance,
  );

  const StyledMenuBox = styled(Box)(({theme}) => ({
    width: '100%',
    boxSizing: 'border-box',
    padding: theme.spacing(2),
  }));

  const sideBarStyle = {
    // to adjust to the navbar height
    maxHeight: 0,
    position: 'sticky',
    minHeight: 'calc(100vh - 56px)',
    flex: 2,

    [theme.breakpoints.up('xs')]: {
      '@media (orientation: landscape)': {
        top: 48,
        minHeight: 'calc(100vh - 48px)',
      },
    },
    [theme.breakpoints.up('sm')]: {
      top: 64,
      minHeight: 'calc(100vh - 64px)',
    },
    maxWidth: 300,
  };

  return (
    <Box sx={sideBarStyle}>
      <StyledMenuBox>
        <nav aria-label={t('sideBar.navTitle')}>
          <List
            sx={{
              textDecoration: 'none',
              borderRadius: theme.borderRadius.default,
              backgroundColor: theme.interface.main,
              boxShadow: 1,
            }}
            subheader={
              <ListSubheader
                sx={{
                  backgroundColor: theme.interface.border,
                  borderRadius: '4px 4px 0px 0px',
                  padding: theme.spacing(1.5),
                  paddingLeft: theme.spacing(2.5),
                }}
              >
                <Typography
                  variant="h6"
                  fontWeight={theme.typography.fontWeightLight}
                  color={theme.palette.text.primary}
                >
                  {t('layout.Menu')}
                </Typography>
              </ListSubheader>
            }
          >
            <MenuItem
              to={routeHome}
              icon={<HomeOutlined />}
              text="layout.Home"
            />
            <MenuItem
              to={routeMarket}
              icon={<StorefrontOutlined />}
              text="layout.MarketPlace"
            />

            <MenuItem
              to={routeWallet}
              icon={<WalletOutlined />}
              text="layout.Wallet"
            />
          </List>
        </nav>
      </StyledMenuBox>
      <StyledMenuBox>
        {location.pathname !== routeWallet && (
          <nav aria-label={t('sideBar.navTitle')}>
            <List
              sx={{
                textDecoration: 'none',
                borderRadius: theme.borderRadius.default,
                backgroundColor: theme.interface.main,
                boxShadow: 1,
              }}
              subheader={
                <ListSubheader
                  sx={{
                    backgroundColor: theme.interface.border,
                    borderRadius: '4px 4px 0px 0px',
                    padding: theme.spacing(1.5),
                    paddingLeft: theme.spacing(2.5),
                  }}
                >
                  <Typography
                    variant="h6"
                    fontWeight={theme.typography.fontWeightLight}
                    color={theme.palette.text.primary}
                  >
                    {t('layout.MyBalance')}
                  </Typography>
                </ListSubheader>
              }
            >
              <ListItemButton component={NavLink} to={routeWallet}>
                <ListItemIcon data-testid="toSpend" sx={{width: 100}}>
                  {t('layout.ToSpend')}
                </ListItemIcon>
                {status === 'loading' ? (
                  <CircularProgress
                    data-testid="awardCircularProgress"
                    size="2rem"
                  />
                ) : (
                  <ListItemText data-testid="toSpendBalance"
                    primaryTypographyProps={{
                      fontSize: '17px',
                      fontWeight: theme.typography.fontWeightMedium,
                    }}
                    primary={data?.toSpendBalance}
                  />
                )}
              </ListItemButton>
              <Divider variant="middle" />
              <ListItemButton component={NavLink} to={routeWallet}>
                <ListItemIcon data-testid="toAward" sx={{width: 100}}>
                  {t('layout.ToAward')}
                </ListItemIcon>
                {status === 'loading' ? (
                  <CircularProgress
                    data-testid="awardCircularProgress"
                    size="2rem"
                  />
                ) : (
                  <ListItemText data-testid="toAwardBalance"
                    primaryTypographyProps={{
                      fontSize: '17px',
                      fontWeight: theme.typography.fontWeightMedium,
                    }}
                    primary={data?.toAwardBalance}
                  />
                )}
              </ListItemButton>
            </List>
          </nav>
        )}
      </StyledMenuBox>
    </Box>
  );
}

export default SideBar;

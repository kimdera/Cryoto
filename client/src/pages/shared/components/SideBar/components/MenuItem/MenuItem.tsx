import {
  ListItemButton,
  ListItemIcon,
  ListItemText,
  useTheme,
} from '@mui/material';
import {NavLink} from 'react-router-dom';
import {useTranslation} from 'react-i18next';

interface MenuItemProps {
  icon: React.ReactNode;
  text: string;
  to: string;
}

function MenuItem(props: MenuItemProps) {
  const {icon, text, to} = props;
  const {t} = useTranslation();
  const theme = useTheme();

  const ItemListButtonStyle = {
    '&.Mui-selected': {
      color: theme.palette.primary.main,
      backgroundColor: 'transparent',
    },
  };

  return (
    <ListItemButton data-testid="listItemButton"
      component={NavLink}
      to={to}
      selected={location.pathname === to}
      sx={ItemListButtonStyle}
    >
      <ListItemIcon
        sx={{
          color: 'inherit',
        }}
      >
        {icon}
      </ListItemIcon>
      <ListItemText
        primaryTypographyProps={{
          fontSize: '16px',
          fontWeight: theme.typography.fontWeightMedium,
        }}
        primary={t(text)}
      />
    </ListItemButton>
  );
}

export default MenuItem;
